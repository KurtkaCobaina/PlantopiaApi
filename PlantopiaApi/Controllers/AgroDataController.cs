// Controllers/AgroDataController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantopiaApi.Data;
using PlantopiaApi.Models;

namespace PlantopiaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgroDataController : ControllerBase
    {
        private readonly PlantopiaDbContext _context;

        public AgroDataController(PlantopiaDbContext context)
        {
            _context = context;
        }

        [HttpGet("crops")]
        public async Task<ActionResult<List<Crop>>> GetCrops()
        {
            return await _context.Crops.ToListAsync();
        }

        [HttpGet("soil-types")]
        public async Task<ActionResult<List<SoilType>>> GetSoilTypes()
        {
            return await _context.SoilTypes.ToListAsync();
        }
    }
}