using System.ComponentModel.DataAnnotations;

namespace DoctorAppointments.Server.ViewModels;

public class DepartmentFormViewModel
{
	[Required(ErrorMessage = "Department name is required.")]
	[StringLength(100)]
	public string Name { get; set; } = string.Empty;

	[StringLength(1000)]
	public string? Description { get; set; }

	[Display(Name = "Active")]
	public bool IsActive { get; set; } = true;
}