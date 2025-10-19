using System.ComponentModel.DataAnnotations;

namespace Api.DTOs
{
    // Yeni mesaj oluşturma isteği modeli.
    public class CreateMessageRequest
    {
        [Required]
        [MinLength(1)]
        [MaxLength(1000)]
        public string Text { get; set; } = string.Empty; // Mesaj içeriği
    }
}
