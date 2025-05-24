using Microsoft.EntityFrameworkCore;
using Tutorial11.Models;

namespace Tutorial11.Data;

public class DatabaseContext : DbContext
{
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Medicament> Medicaments { get; set; }
    public DbSet<Prescription> Prescriptions { get; set; }
    public DbSet<PrescriptionMedicament> PrescriptionMedicaments { get; set; }

    protected DatabaseContext() { }

    public DatabaseContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PrescriptionMedicament>()
            .HasKey(pm => new { pm.IdPrescription, pm.IdMedicament });

        modelBuilder.Entity<PrescriptionMedicament>()
            .HasOne(pm => pm.Prescription)
            .WithMany(p => p.PrescriptionMedicaments)
            .HasForeignKey(pm => pm.IdPrescription);

        modelBuilder.Entity<PrescriptionMedicament>()
            .HasOne(pm => pm.Medicament)
            .WithMany(m => m.PrescriptionMedicaments)
            .HasForeignKey(pm => pm.IdMedicament);

        modelBuilder.Entity<Doctor>().HasData(new List<Doctor>
        {
            new Doctor { IdDoctor = 1, FirstName = "Gregory", LastName = "House", Email = "house@example.com" },
            new Doctor { IdDoctor = 2, FirstName = "Meredith", LastName = "Grey", Email = "grey@example.com" }
        });

        modelBuilder.Entity<Medicament>().HasData(new List<Medicament>
        {
            new Medicament { IdMedicament = 1, Name = "Ibuprofen", Description = "Painkiller", Type = "Tablet" },
            new Medicament { IdMedicament = 2, Name = "Paracetamol", Description = "Fever reducer", Type = "Tablet" }
        });

        modelBuilder.Entity<Patient>().HasData(new List<Patient>
        {
            new Patient { IdPatient = 1, FirstName = "John", LastName = "Doe", Birthdate = new DateTime(1990, 5, 20) }
        });
    }
}
