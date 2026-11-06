using DoctorAppointments.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DoctorAppointments.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    public DbSet<AppointmentSlot> AppointmentSlots =>
        Set<AppointmentSlot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(department => department.Id);

            entity.Property(department => department.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(department => department.Name)
                .IsUnique();

            entity.Property(department => department.Description)
                .HasMaxLength(1000);
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(doctor => doctor.Id);

            entity.Property(doctor => doctor.FullName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(doctor => doctor.Qualification)
                .HasMaxLength(250)
                .IsRequired();

            entity.Property(doctor => doctor.About)
                .HasMaxLength(2000);

            entity.Property(doctor => doctor.ConsultationFee)
                .HasPrecision(10, 2);

            entity.HasOne(doctor => doctor.Department)
                .WithMany(department => department.Doctors)
                .HasForeignKey(doctor => doctor.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.ToTable("Doctors", table =>
            {
                table.HasCheckConstraint(
                    "CK_Doctors_ConsultationFee",
                    "[ConsultationFee] >= 0");

                table.HasCheckConstraint(
                    "CK_Doctors_ExperienceYears",
                    "[ExperienceYears] >= 0");
            });
        });

        modelBuilder.Entity<AppointmentSlot>(entity =>
        {
            entity.HasKey(slot => slot.Id);

            entity.HasIndex(slot => new
            {
                slot.DoctorId,
                slot.StartsAtUtc
            })
            .IsUnique();

            entity.HasOne(slot => slot.Doctor)
                .WithMany()
                .HasForeignKey(slot => slot.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);
        });


        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(appointment => appointment.Id);

            entity.HasIndex(appointment => appointment.AppointmentSlotId)
                .IsUnique();

            entity.Property(appointment => appointment.PatientUserId)
                .HasMaxLength(450)
                .IsRequired();

            entity.Property(appointment => appointment.ConsultationFee)
                .HasPrecision(10, 2);

            entity.HasOne(appointment => appointment.Slot)
                .WithMany()
                .HasForeignKey(appointment => appointment.AppointmentSlotId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey(appointment => appointment.PatientUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });



    }
}