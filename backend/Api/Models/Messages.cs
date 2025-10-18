using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class Message
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(1000)]
        public string Text { get; set; } = string.Empty;
        [Required]
        [MaxLength(20)]
        public string Sentiment { get; set; } = string.Empty;
        public double Score { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
