using System.Security.Claims;
using DoctorAppointments.Core.Entities;
using DoctorAppointments.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DoctorAppointments.Server.Controllers;

[Authorize]
public class AppointmentsController : Controller
{
    private readonly AppDbContext _db;

    public AppointmentsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Challenge();

        var appointments = await _db.Appointments
            .AsNoTracking()
            .Include(appointment => appointment.Slot)
                .ThenInclude(slot => slot.Doctor)
            .Where(appointment => appointment.PatientUserId == userId)
            .OrderByDescending(appointment => appointment.Slot.StartsAtUtc)
            .ToListAsync();

        return View(appointments);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(int slotId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Challenge();

        var now = DateTimeOffset.UtcNow;

        var slot = await _db.AppointmentSlots
            .AsNoTracking()
            .Where(slot =>
                slot.Id == slotId &&
                slot.StartsAtUtc > now &&
                slot.Doctor.IsActive &&
                slot.Doctor.Department.IsActive)
            .Select(slot => new
            {
                slot.Id,
                slot.DoctorId,
                slot.Doctor.ConsultationFee
            })
            .SingleOrDefaultAsync();

        if (slot is null)
        {
            TempData["Error"] = "This slot is no longer available.";

            return RedirectToAction(
                "Index", "DoctorDirectory");
        }

        var existing = await _db.Appointments
            .AsNoTracking()
            .SingleOrDefaultAsync(appointment =>
                appointment.AppointmentSlotId == slotId);

        if (existing is not null)
        {
            if (existing.PatientUserId == userId)
            {
                TempData["Success"] = "You already booked this slot.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Another patient has booked this slot.";

            return RedirectToAction(
                "Slots", "DoctorDirectory", new { id = slot.DoctorId });
        }

        _db.Appointments.Add(new Appointment
        {
            AppointmentSlotId = slot.Id,
            PatientUserId = userId,
            ConsultationFee = slot.ConsultationFee,
            BookedAtUtc = DateTimeOffset.UtcNow
        });

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is SqlException sql &&
            (sql.Number == 2601 || sql.Number == 2627))
        {
            var bookedByCurrentUser = await _db.Appointments
                .AsNoTracking()
                .AnyAsync(appointment =>
                    appointment.AppointmentSlotId == slotId &&
                    appointment.PatientUserId == userId);

            if (bookedByCurrentUser)
            {
                TempData["Success"] = "You already booked this slot.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Another patient has booked this slot.";

            return RedirectToAction(
                "Slots", "DoctorDirectory", new { id = slot.DoctorId });
        }

        TempData["Success"] = "Appointment booked successfully.";

        return RedirectToAction(nameof(Index));
    }
}