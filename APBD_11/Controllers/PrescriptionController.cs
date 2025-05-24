using Microsoft.AspNetCore.Mvc;
using Tutorial11.DTOs;
using Tutorial11.Services;
namespace Tutorial11.Controllers; 

    [Route("api/[controller]")]
    [ApiController]
    public class PrescriptionController : ControllerBase
    {
        private readonly IPrescriptionService _prescriptionService;

        public PrescriptionController(IPrescriptionService prescriptionService)
        {
            _prescriptionService = prescriptionService;
        }

        // POST: api/prescription
        [HttpPost]
        public async Task<IActionResult> AddPrescription([FromBody] AddPrescriptionDto dto)
        {
            try
            {
                await _prescriptionService.AddPrescription(dto);
                return Ok(new { message = "Prescription added successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        // GET: api/prescription/patient/1
        [HttpGet("patient/{id}")]
        public async Task<IActionResult> GetPatientData(int id)
        {
            try
            {
                var result = await _prescriptionService.GetPatientData(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
     
    }
}