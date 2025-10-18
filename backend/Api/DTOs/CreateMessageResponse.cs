namespace Api.DTOs
{
    public class CreateMessageResponse
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Sentiment { get; set; } = string.Empty;
        public double Score { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
