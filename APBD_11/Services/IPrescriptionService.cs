
using Tutorial11.DTOs;

namespace Tutorial11.Services;

public interface IPrescriptionService
{
    Task AddPrescription(AddPrescriptionDto dto);
    Task<PatientResponseDto> GetPatientData(int patientId);
}