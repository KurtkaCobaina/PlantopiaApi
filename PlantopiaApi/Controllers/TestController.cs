using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantopiaApi.Data;
using PlantopiaApi.Models;

namespace PlantopiaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly PlantopiaDbContext _context;

        public TestController(PlantopiaDbContext context)
        {
            _context = context;
        }

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            return await _context.Users.ToListAsync();
        }

        [HttpGet("tasks")]
        public async Task<ActionResult<IEnumerable<UserTask>>> GetAllTasks()
        {
            return await _context.UserTasks.ToListAsync();
        }
        [HttpGet("consultations")]
        public async Task<ActionResult<IEnumerable<Consultation>>> GetAllConsultations()
        {
            return await _context.Consultations.ToListAsync();
        }
        [HttpGet("diagnoses")]
        public async Task<ActionResult<IEnumerable<Diagnosis>>> GetAllDiagnoses()
        {
            return await _context.Diagnoses.ToListAsync();
        }

        [HttpGet("experts")]
        public async Task<ActionResult<IEnumerable<Expert>>> GetAllExperts()
        {
            return await _context.Experts.ToListAsync();
        }
        [HttpGet("fertilizer_calculations")]
        public async Task<ActionResult<IEnumerable<FertilizerCalculation>>> GetAllFertilizerCalculations()
        {
            return await _context.FertilizerCalculations.ToListAsync();
        }
        [HttpGet("ndvi_maps")]
        public async Task<ActionResult<IEnumerable<NdviMap>>> GetAllNdviMaps()
        {
            return await _context.NdviMaps.ToListAsync();
        }
        [HttpGet("soil_tests")]
        public async Task<ActionResult<IEnumerable<SoilTest>>> GetAllSoilTests()
        {
            return await _context.SoilTests.ToListAsync();
        }
    }
}