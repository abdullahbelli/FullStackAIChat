using Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly AppDbContext _db;

        public HealthController(AppDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken ct)
        {
            var dbOk = true;
            try
            {
                // Hafif bağlantı testi
                dbOk = await _db.Database.CanConnectAsync(ct);
            }
            catch
            {
                dbOk = false;
            }

            return Ok(new
            {
                status = "ok",
                db = dbOk ? "ok" : "down",
                utc = DateTime.UtcNow
            });
        }
    }
}
