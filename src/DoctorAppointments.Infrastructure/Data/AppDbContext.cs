using DoctorAppointments.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DoctorAppointments.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<Doctor> Doctors => Set<Doctor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(x => x.Name)
                .IsUnique();

            entity.Property(x => x.Description)
                .HasMaxLength(1000);
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.FullName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.Qualification)
                .HasMaxLength(250)
                .IsRequired();

            entity.Property(x => x.About)
                .HasMaxLength(2000);

            entity.Property(x => x.ConsultationFee)
                .HasPrecision(10, 2);

            entity.HasOne(x => x.Department)
                .WithMany(x => x.Doctors)
                .HasForeignKey(x => x.DepartmentId)
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
    }
}