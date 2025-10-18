using Api.DTOs;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _svc;

        public MessagesController(IMessageService svc) => _svc = svc;

        /// <summary>Yeni mesaj oluşturur, AI ile duygu analizi yapar ve sonucu döner.</summary>
        [HttpPost]
        public async Task<ActionResult<CreateMessageResponse>> Create([FromBody] CreateMessageRequest req, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            try
            {
                var result = await _svc.CreateAsync(req.Text, ct);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                // Örn. boş/çok uzun metin vb.
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                // AI servisi erişilemezse MessageService nötr kayda düşecek şekilde tasarlandı.
                // Yine de beklenmeyen hata için 502 dönmek istersen:
                // return StatusCode(502, new { message = "Sentiment service unavailable." });
                throw;
            }
        }

        /// <summary>En yeni mesajları listeler (varsayılan 50 kayıt).</summary>
        [HttpGet]
        public async Task<ActionResult<ListMessagesResponse>> List([FromQuery] int take = 50, [FromQuery] int skip = 0, CancellationToken ct = default)
        {
            var result = await _svc.ListAsync(take, skip, ct);
            return Ok(result);
        }
    }
}
