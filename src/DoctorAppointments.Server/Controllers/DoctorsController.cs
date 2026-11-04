using DoctorAppointments.Core.Entities;
using DoctorAppointments.Infrastructure.Data;
using DoctorAppointments.Server.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace DoctorAppointments.Server.Controllers;

[Authorize(Roles = "Admin")]
public class DoctorsController : Controller
{
    private readonly AppDbContext _db;

    public DoctorsController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var doctors = await _db.Doctors
            .AsNoTracking()
            .Include(x => x.Department)
            .OrderBy(x => x.FullName)
            .ToListAsync();

        return View(doctors);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new DoctorFormViewModel();

        await LoadDepartmentsAsync(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        DoctorFormViewModel model)
    {
        Normalize(model);

        await ValidateDepartmentAsync(model);

        if (!ModelState.IsValid)
        {
            await LoadDepartmentsAsync(model);
            return View(model);
        }

        var doctor = new Doctor();

        ApplyChanges(doctor, model);

        _db.Doctors.Add(doctor);

        await _db.SaveChangesAsync();

        TempData["Success"] = "Doctor successfully added.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var doctor = await _db.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (doctor is null)
            return NotFound();

        ViewData["DoctorId"] = id;

        var model = new DoctorFormViewModel
        {
            FullName = doctor.FullName,
            Qualification = doctor.Qualification,
            About = doctor.About,
            ExperienceYears = doctor.ExperienceYears,
            ConsultationFee = doctor.ConsultationFee,
            DepartmentId = doctor.DepartmentId,
            IsActive = doctor.IsActive
        };

        await LoadDepartmentsAsync(model, doctor.DepartmentId);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id, DoctorFormViewModel model)
    {
        var doctor = await _db.Doctors.FindAsync(id);

        if (doctor is null)
            return NotFound();

        ViewData["DoctorId"] = id;

        var currentDepartmentId = doctor.DepartmentId;

        Normalize(model);

        await ValidateDepartmentAsync(model, currentDepartmentId);

        if (!ModelState.IsValid)
        {
            await LoadDepartmentsAsync(model, currentDepartmentId);
            return View(model);
        }

        ApplyChanges(doctor, model);

        await _db.SaveChangesAsync();

        TempData["Success"] = "Doctor details successfully updated.";

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadDepartmentsAsync(
        DoctorFormViewModel model,
        int? currentDepartmentId = null)
    {
        var departments = await _db.Departments
            .AsNoTracking()
            .Where(x => x.IsActive || x.Id == currentDepartmentId)
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.IsActive
            })
            .ToListAsync();

        model.Departments = departments
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.IsActive
                    ? x.Name
                    : $"{x.Name} (Inactive)"
            })
            .ToList();
    }

    private async Task ValidateDepartmentAsync(
        DoctorFormViewModel model,
        int? currentDepartmentId = null)
    {
        if (!model.DepartmentId.HasValue)
            return;

        var allowed = await _db.Departments.AnyAsync(x =>
            x.Id == model.DepartmentId.Value &&
            (x.IsActive || x.Id == currentDepartmentId));

        if (!allowed)
        {
            ModelState.AddModelError(
                nameof(model.DepartmentId),
                "Select  Valid active department .");
        }
    }

    private void Normalize(DoctorFormViewModel model)
    {
        model.FullName = (model.FullName ?? string.Empty).Trim();
        model.Qualification =
            (model.Qualification ?? string.Empty).Trim();
        model.About = model.About?.Trim();

        if (string.IsNullOrWhiteSpace(model.FullName))
        {
            ModelState.AddModelError(
                nameof(model.FullName),
                "Doctor Name is Required.");
        }

        if (string.IsNullOrWhiteSpace(model.Qualification))
        {
            ModelState.AddModelError(
                nameof(model.Qualification),
                "Qualification is required.");
        }
    }

    private static void ApplyChanges(
        Doctor doctor, DoctorFormViewModel model)
    {
        doctor.FullName = model.FullName;
        doctor.Qualification = model.Qualification;
        doctor.About = model.About;
        doctor.ExperienceYears = model.ExperienceYears;
        doctor.ConsultationFee = model.ConsultationFee;
        doctor.DepartmentId = model.DepartmentId!.Value;
        doctor.IsActive = model.IsActive;
    }
}