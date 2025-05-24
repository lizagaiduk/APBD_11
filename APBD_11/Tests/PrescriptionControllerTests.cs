
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tutorial11.Controllers;
using Tutorial11.DTOs;
using Tutorial11.Services;
using Xunit;

namespace Tutorial11.Tests
{
    public class PrescriptionControllerTests
    {
        private readonly Mock<IPrescriptionService> _serviceMock;
        private readonly PrescriptionController _controller;

        public PrescriptionControllerTests()
        {
            _serviceMock = new Mock<IPrescriptionService>();
            _controller = new PrescriptionController(_serviceMock.Object);
        }

        [Fact]
        public async Task AddPrescription_ValidRequest_ReturnsOk()
        {
            var dto = new AddPrescriptionDto
            {
                Date = DateTime.Today,
                DueDate = DateTime.Today.AddDays(1),
                Doctor = new DoctorDto { IdDoctor = 1 },
                Patient = new PatientDto { FirstName = "John", LastName = "Doe", Birthdate = new DateTime(1990, 1, 1) },
                Medicaments = new List<MedicamentDto> {
                    new MedicamentDto { IdMedicament = 1, Dose = 2, Description = "Test" }
                }
            };

            _serviceMock.Setup(s => s.AddPrescription(dto)).Returns(Task.CompletedTask);
            var result = await _controller.AddPrescription(dto);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task AddPrescription_ThrowsError_ReturnsBadRequest()
        {
            var dto = new AddPrescriptionDto();
            _serviceMock.Setup(s => s.AddPrescription(dto)).ThrowsAsync(new Exception("Invalid data"));
            var result = await _controller.AddPrescription(dto);
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequest.StatusCode);
        }

        [Fact]
        public async Task GetPatientData_ValidId_ReturnsOk()
        {
            int patientId = 1;
            var expected = new PatientResponseDto { IdPatient = 1, FirstName = "John", LastName = "Doe" };
            _serviceMock.Setup(s => s.GetPatientData(patientId)).ReturnsAsync(expected);
            var result = await _controller.GetPatientData(patientId);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(expected, okResult.Value);
        }

        [Fact]
        public async Task GetPatientData_NotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetPatientData(It.IsAny<int>())).ThrowsAsync(new Exception("Patient not found"));
            var result = await _controller.GetPatientData(99);
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFound.StatusCode);
        }
    }
}
