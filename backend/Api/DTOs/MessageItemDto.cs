namespace Api.DTOs
{
    public class MessageItemDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Sentiment { get; set; } = string.Empty; // positive | neutral | negative
        public double Score { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
