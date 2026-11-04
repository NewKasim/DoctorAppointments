using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DoctorAppointments.Server.ViewModels;

public class DoctorFormViewModel
{
    [Required]
    [StringLength(150)]
    [Display(Name = "Doctor Name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(250)]
    public string Qualification { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? About { get; set; }

    [Range(0, 80)]
    [Display(Name = "Experience (Years)")]
    public int ExperienceYears { get; set; }

    [Range(typeof(decimal), "0", "99999999.99")]
    [Display(Name = "Consultation Fee")]
    public decimal ConsultationFee { get; set; }

    [Required(ErrorMessage = "Department select karo.")]
    [Range(1, int.MaxValue)]
    [Display(Name = "Department")]
    public int? DepartmentId { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [ValidateNever]
    public List<SelectListItem> Departments { get; set; } = new();
}