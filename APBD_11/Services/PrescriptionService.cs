using Microsoft.EntityFrameworkCore;
using Tutorial11.Data;
using Tutorial11.DTOs;
using Tutorial11.Models;

namespace Tutorial11.Services
{
    public class PrescriptionService : IPrescriptionService
    {
        private readonly DatabaseContext _context;

        public PrescriptionService(DatabaseContext context)
        {
            _context = context;
        }

        public async Task AddPrescription(AddPrescriptionDto dto)
        {
            if (dto.Medicaments.Count > 10)
                throw new Exception("A prescription can include a maximum of 10 medications.");
            if (dto.DueDate < dto.Date)
                throw new Exception("DueDate cannot be earlier than Date.");
            Doctor doctor;
            if (dto.Doctor.IdDoctor > 0)
            {
                doctor = await _context.Doctors.FindAsync(dto.Doctor.IdDoctor);
                if (doctor == null)
                    throw new Exception("Doctor with given ID not found.");
            }
            else
            {
                doctor = await _context.Doctors.FirstOrDefaultAsync(d =>
                    d.FirstName == dto.Doctor.FirstName &&
                    d.LastName == dto.Doctor.LastName &&
                    d.Email == dto.Doctor.Email);
                if (doctor == null)
                    throw new Exception("Doctor not found.");
            }
            var medicamentIds = dto.Medicaments.Select(m => m.IdMedicament).ToList();
            var existing = await _context.Medicaments
                .Where(m => medicamentIds.Contains(m.IdMedicament))
                .Select(m => m.IdMedicament)
                .ToListAsync();
            if (existing.Count != medicamentIds.Count)
                throw new Exception("One or more medicaments not found.");
            Patient patient = null;
            if (dto.Patient.IdPatient.HasValue && dto.Patient.IdPatient > 0)
            {
                patient = await _context.Patients.FindAsync(dto.Patient.IdPatient.Value);
                if (patient == null)
                    throw new Exception($"Patient with ID {dto.Patient.IdPatient} not found.");
            }
            else
            {
                patient = await _context.Patients.FirstOrDefaultAsync(p =>
                    p.FirstName == dto.Patient.FirstName &&
                    p.LastName == dto.Patient.LastName &&
                    p.Birthdate == dto.Patient.Birthdate);

                if (patient == null)
                {
                    patient = new Patient
                    {
                        FirstName = dto.Patient.FirstName,
                        LastName = dto.Patient.LastName,
                        Birthdate = dto.Patient.Birthdate
                    };
                    _context.Patients.Add(patient);
                    await _context.SaveChangesAsync();
                }
            }
            var prescription = new Prescription
            {
                Date = dto.Date,
                DueDate = dto.DueDate,
                IdDoctor = doctor.IdDoctor,
                IdPatient = patient.IdPatient
            };
            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();
            foreach (var m in dto.Medicaments)
            {
                _context.PrescriptionMedicaments.Add(new PrescriptionMedicament
                {
                    IdPrescription = prescription.IdPrescription,
                    IdMedicament = m.IdMedicament,
                    Dose = m.Dose,
                    Description = m.Description
                });
            }

            await _context.SaveChangesAsync();
        }
        public async Task<PatientResponseDto> GetPatientData(int patientId)
        {
            var patient = await _context.Patients
                .Include(p => p.Prescriptions)
                .ThenInclude(p => p.PrescriptionMedicaments)
                .ThenInclude(pm => pm.Medicament)
                .Include(p => p.Prescriptions)
                .ThenInclude(p => p.Doctor)
                .FirstOrDefaultAsync(p => p.IdPatient == patientId);

            if (patient == null)
                throw new Exception("Patient not found.");

            return new PatientResponseDto
            {
                IdPatient = patient.IdPatient,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                Birthdate = patient.Birthdate,
                Prescriptions = patient.Prescriptions
                    .OrderBy(p => p.DueDate)
                    .Select(p => new PrescriptionWithDetailsDto
                    {
                        IdPrescription = p.IdPrescription,
                        Date = p.Date,
                        DueDate = p.DueDate,
                        Doctor = new DoctorShortDto
                        {
                            IdDoctor = p.Doctor.IdDoctor,
                            FirstName = p.Doctor.FirstName
                        },
                        Medicaments = p.PrescriptionMedicaments.Select(pm => new MedicamentDetailsDto
                        {
                            IdMedicament = pm.Medicament.IdMedicament,
                            Name = pm.Medicament.Name,
                            Dose = pm.Dose,
                            Description = pm.Description
                        }).ToList()
                    }).ToList()
            };
        }
    }
}
