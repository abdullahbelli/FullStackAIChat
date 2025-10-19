using Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    // API ve veritabanı bağlantı durumunu kontrol eden endpoint
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
                // Veritabanı bağlantısı testi
                dbOk = await _db.Database.CanConnectAsync(ct);
            }
            catch
            {
                dbOk = false;
            }

            return Ok(new
            {
                status = "ok",           // API durumu
                db = dbOk ? "ok" : "down", // Veritabanı durumu
                utc = DateTime.UtcNow      // Sunucu UTC zamanı
            });
        }
    }
}
