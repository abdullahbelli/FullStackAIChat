namespace AiService
{
    public interface ISentimentClient
    {
        Task<(string label, double score)> AnalyzeAsync(string text, CancellationToken ct = default);
    }
}
