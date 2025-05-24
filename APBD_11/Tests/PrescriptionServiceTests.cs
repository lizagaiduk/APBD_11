
using Microsoft.EntityFrameworkCore;
using Tutorial11.Data;
using Tutorial11.DTOs;
using Tutorial11.Models;
using Tutorial11.Services;
using Xunit;

namespace Tutorial11.Tests
{
    public class PrescriptionServiceTests
    {
        private readonly DbContextOptions<DatabaseContext> _dbContextOptions;

        public PrescriptionServiceTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task AddPrescription_ValidInput_AddsSuccessfully()
        {
            using var context = new DatabaseContext(_dbContextOptions);
            context.Doctors.Add(new Doctor
                { IdDoctor = 1, FirstName = "Greg", LastName = "House", Email = "house@example.com" });
            context.Medicaments.Add(new Medicament
                { IdMedicament = 1, Name = "MedA", Description = "Desc", Type = "Tab" });
            await context.SaveChangesAsync();

            var service = new PrescriptionService(context);

            var dto = new AddPrescriptionDto
            {
                Date = DateTime.Today,
                DueDate = DateTime.Today.AddDays(5),
                Doctor = new DoctorDto { IdDoctor = 1 },
                Patient = new PatientDto { FirstName = "John", LastName = "Doe", Birthdate = new DateTime(1990, 1, 1) },
                Medicaments = new List<MedicamentDto>
                {
                    new MedicamentDto { IdMedicament = 1, Dose = 2, Description = "Take after meal" }
                }
            };

            await service.AddPrescription(dto);

            Assert.Single(context.Prescriptions);
        }

        [Fact]
        public async Task AddPrescription_TooManyMedicaments_ThrowsException()
        {
            using var context = new DatabaseContext(_dbContextOptions);
            var service = new PrescriptionService(context);

            var dto = new AddPrescriptionDto
            {
                Date = DateTime.Today,
                DueDate = DateTime.Today.AddDays(1),
                Doctor = new DoctorDto { IdDoctor = 1 },
                Patient = new PatientDto { FirstName = "Jane", LastName = "Smith", Birthdate = DateTime.Today },
                Medicaments = Enumerable.Range(1, 11).Select(i => new MedicamentDto
                {
                    IdMedicament = i,
                    Dose = 1,
                    Description = "Over limit"
                }).ToList()
            };

            await Assert.ThrowsAsync<Exception>(() => service.AddPrescription(dto));
        }

        [Fact]
        public async Task AddPrescription_DueDateBeforeDate_ThrowsException()
        {
            using var context = new DatabaseContext(_dbContextOptions);
            var service = new PrescriptionService(context);

            var dto = new AddPrescriptionDto
            {
                Date = DateTime.Today,
                DueDate = DateTime.Today.AddDays(-1),
                Doctor = new DoctorDto { IdDoctor = 1 },
                Patient = new PatientDto { FirstName = "Anna", LastName = "Smith", Birthdate = DateTime.Today },
                Medicaments = new List<MedicamentDto>()
            };

            await Assert.ThrowsAsync<Exception>(() => service.AddPrescription(dto));
        }

        [Fact]
        public async Task AddPrescription_MedicamentNotFound_ThrowsException()
        {
            using var context = new DatabaseContext(_dbContextOptions);
            context.Doctors.Add(new Doctor
                { IdDoctor = 1, FirstName = "Greg", LastName = "House", Email = "house@example.com" });
            await context.SaveChangesAsync();

            var service = new PrescriptionService(context);

            var dto = new AddPrescriptionDto
            {
                Date = DateTime.Today,
                DueDate = DateTime.Today.AddDays(1),
                Doctor = new DoctorDto { IdDoctor = 1 },
                Patient = new PatientDto { FirstName = "Jake", LastName = "Smith", Birthdate = DateTime.Today },
                Medicaments = new List<MedicamentDto>
                {
                    new MedicamentDto { IdMedicament = 99, Dose = 1, Description = "Invalid" }
                }
            };

            await Assert.ThrowsAsync<Exception>(() => service.AddPrescription(dto));
        }

        [Fact]
        public async Task AddPrescription_CreatesNewPatient_WhenNotFound()
        {
            using var context = new DatabaseContext(_dbContextOptions);
            context.Doctors.Add(new Doctor
                { IdDoctor = 1, FirstName = "Greg", LastName = "House", Email = "house@example.com" });
            context.Medicaments.Add(new Medicament
                { IdMedicament = 1, Name = "MedB", Description = "Desc", Type = "Tab" });
            await context.SaveChangesAsync();

            var service = new PrescriptionService(context);

            var dto = new AddPrescriptionDto
            {
                Date = DateTime.Today,
                DueDate = DateTime.Today.AddDays(5),
                Doctor = new DoctorDto { IdDoctor = 1 },
                Patient = new PatientDto
                    { FirstName = "Alex", LastName = "Brown", Birthdate = new DateTime(1985, 3, 15) },
                Medicaments = new List<MedicamentDto>
                {
                    new MedicamentDto { IdMedicament = 1, Dose = 1, Description = "Take once" }
                }
            };

            await service.AddPrescription(dto);

            var createdPatient =
                await context.Patients.FirstOrDefaultAsync(p => p.FirstName == "Alex" && p.LastName == "Brown");
            Assert.NotNull(createdPatient);
        }

        [Fact]
        public async Task GetPatientData_ValidId_ReturnsData()
        {
            using var context = new DatabaseContext(_dbContextOptions);
            var patient = new Patient
                { IdPatient = 1, FirstName = "Tom", LastName = "Lee", Birthdate = new DateTime(1980, 1, 1) };
            var doctor = new Doctor { IdDoctor = 1, FirstName = "Doc", LastName = "Tor", Email = "doc@tor.com" };
            var medicament = new Medicament { IdMedicament = 1, Name = "M1", Description = "Pain", Type = "Tab" };
            var prescription = new Prescription
            {
                IdPrescription = 1, IdDoctor = 1, IdPatient = 1, Date = DateTime.Today,
                DueDate = DateTime.Today.AddDays(1)
            };
            var prescriptionMed = new PrescriptionMedicament
                { IdPrescription = 1, IdMedicament = 1, Dose = 1, Description = "Daily" };

            context.Patients.Add(patient);
            context.Doctors.Add(doctor);
            context.Medicaments.Add(medicament);
            context.Prescriptions.Add(prescription);
            context.PrescriptionMedicaments.Add(prescriptionMed);
            await context.SaveChangesAsync();

            var service = new PrescriptionService(context);
            var result = await service.GetPatientData(1);

            Assert.NotNull(result);
            Assert.Equal("Tom", result.FirstName);
            Assert.Single(result.Prescriptions);
            Assert.Equal("M1", result.Prescriptions[0].Medicaments[0].Name);
        }

        [Fact]
        public async Task GetPatientData_InvalidId_ThrowsException()
        {
            using var context = new DatabaseContext(_dbContextOptions);
            var service = new PrescriptionService(context);

            await Assert.ThrowsAsync<Exception>(() => service.GetPatientData(999));
        }
    }
}
