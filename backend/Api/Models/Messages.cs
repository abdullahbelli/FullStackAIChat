using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    // Veritabanında saklanan mesaj varlığı.
    public class Message
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Text { get; set; } = string.Empty;   // Mesaj içeriği

        [Required]
        [MaxLength(20)]
        public string Sentiment { get; set; } = string.Empty; // Duygu etiketi

        public double Score { get; set; }                      // Duygu puanı
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Oluşturulma zamanı (UTC)
    }
}
