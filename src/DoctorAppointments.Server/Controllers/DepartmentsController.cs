using DoctorAppointments.Core.Entities;
using DoctorAppointments.Infrastructure.Data;
using DoctorAppointments.Server.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DoctorAppointments.Server.Controllers;

[Authorize(Roles = "Admin")]
public class DepartmentsController : Controller
{
    private readonly AppDbContext _db;

    public DepartmentsController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var departments = await _db.Departments
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync();

        return View(departments);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new DepartmentFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        DepartmentFormViewModel model)
    {
        model.Name = (model.Name ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError(
                nameof(model.Name),
                "Enter a valid department name.");
        }

        if (!ModelState.IsValid)
            return View(model);

        var exists = await _db.Departments
            .AnyAsync(x => x.Name == model.Name);

        if (exists)
        {
            ModelState.AddModelError(
                nameof(model.Name),
                "This department name is already in use.");

            return View(model);
        }

        var department = new Department
        {
            Name = model.Name,
            Description = model.Description?.Trim(),
            IsActive = model.IsActive
        };

        _db.Departments.Add(department);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsDuplicate(ex))
        {
            ModelState.AddModelError(
                nameof(model.Name),
                "This department name is already in use.");

            return View(model);
        }

        TempData["Success"] = "Department added successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var department = await _db.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (department is null)
            return NotFound();

        ViewData["DepartmentId"] = department.Id;

        return View(new DepartmentFormViewModel
        {
            Name = department.Name,
            Description = department.Description,
            IsActive = department.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        DepartmentFormViewModel model)
    {
        var department = await _db.Departments.FindAsync(id);

        if (department is null)
            return NotFound();

        ViewData["DepartmentId"] = id;

        model.Name = (model.Name ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError(
                nameof(model.Name),
                "Enter a valid department name.");
        }

        if (!ModelState.IsValid)
            return View(model);

        var exists = await _db.Departments
            .AnyAsync(x => x.Name == model.Name && x.Id != id);

        if (exists)
        {
            ModelState.AddModelError(
                nameof(model.Name),
                "This department name is already in use.");

            return View(model);
        }

        department.Name = model.Name;
        department.Description = model.Description?.Trim();
        department.IsActive = model.IsActive;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsDuplicate(ex))
        {
            ModelState.AddModelError(
                nameof(model.Name),
                "This department name is already in use.");

            return View(model);
        }

        TempData["Success"] = "Department updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    private static bool IsDuplicate(DbUpdateException exception)
    {
        return exception.InnerException is SqlException sql
            && (sql.Number == 2601 || sql.Number == 2627);
    }
}