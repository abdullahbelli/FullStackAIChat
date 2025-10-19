using Api.DTOs;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    // Mesaj oluşturma ve listeleme işlemlerini yöneten API controller.
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _svc;

        public MessagesController(IMessageService svc) => _svc = svc;

        // Yeni mesaj oluşturur.
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
                // Geçersiz parametre hatası.
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                // Diğer hatalar framework tarafından ele alınır.
                throw;
            }
        }

        // Mesaj listesini döndürür.
        [HttpGet]
        public async Task<ActionResult<ListMessagesResponse>> List([FromQuery] int take = 50, [FromQuery] int skip = 0, CancellationToken ct = default)
        {
            var result = await _svc.ListAsync(take, skip, ct);
            return Ok(result);
        }
    }
}
