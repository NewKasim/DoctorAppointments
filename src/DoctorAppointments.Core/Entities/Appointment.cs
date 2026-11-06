namespace DoctorAppointments.Core.Entities;

public class Appointment
{
    public int Id { get; set; }

    public int AppointmentSlotId { get; set; }

    public AppointmentSlot Slot { get; set; } = null!;

    public string PatientUserId { get; set; } = string.Empty;

    public decimal ConsultationFee { get; set; }

    public DateTimeOffset BookedAtUtc { get; set; }
}