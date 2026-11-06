namespace DoctorAppointments.Core.Entities;

public class AppointmentSlot
{
    public int Id { get; set; }

    public int DoctorId { get; set; }

    public Doctor Doctor { get; set; } = null!;

    public DateTimeOffset StartsAtUtc { get; set; }
}