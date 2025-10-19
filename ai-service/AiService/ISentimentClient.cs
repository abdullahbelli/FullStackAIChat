namespace AiService
{
    // Metin  duygu analizi  arayüzü.
    public interface ISentimentClient
    {
        // Metnin duygu etiketini ve puanını döndürür.
        Task<(string label, double score)> AnalyzeAsync(string text, CancellationToken ct = default);
    }
}
