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
}