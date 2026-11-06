using DoctorAppointments.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DoctorAppointments.Server.Controllers;

[Authorize]
public class DoctorDirectoryController : Controller
{
    private readonly AppDbContext _db;

    public DoctorDirectoryController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? departmentId)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var departments = await _db.Departments
            .AsNoTracking()
            .Where(department => department.IsActive)
            .OrderBy(department => department.Name)
            .Select(department => new { department.Id, department.Name })
            .ToListAsync();

        if (departmentId.HasValue &&
            !departments.Any(department => department.Id == departmentId.Value))
        {
            return NotFound();
        }

        ViewBag.Departments = new SelectList(
            departments, "Id", "Name", departmentId);

        var query = _db.Doctors
            .AsNoTracking()
            .Include(doctor => doctor.Department)
            .Where(doctor => doctor.IsActive && doctor.Department.IsActive);

        if (departmentId.HasValue)
            query = query.Where(doctor => doctor.DepartmentId == departmentId.Value);

        var doctors = await query
            .OrderBy(doctor => doctor.FullName)
            .ToListAsync();

        return View(doctors);
    }

    // future slot feature

    [HttpGet]
    public async Task<IActionResult> Slots(int id)
    {
        var doctor = await _db.Doctors
            .AsNoTracking()
            .Where(doctor =>
                doctor.Id == id &&
                doctor.IsActive &&
                doctor.Department.IsActive)
            .Select(doctor => new { doctor.FullName })
            .SingleOrDefaultAsync();

        if (doctor is null)
            return NotFound();

        var now = DateTimeOffset.UtcNow;

        var slots = await _db.AppointmentSlots
            .AsNoTracking()
            .Where(slot =>
                slot.DoctorId == id &&
                slot.StartsAtUtc > now &&
                slot.Doctor.IsActive &&
                slot.Doctor.Department.IsActive &&
                !_db.Appointments.Any(appointment =>
                    appointment.AppointmentSlotId == slot.Id))
            .OrderBy(slot => slot.StartsAtUtc)
            .ToListAsync();

        ViewData["DoctorName"] = doctor.FullName;

        return View(slots);
    }











}