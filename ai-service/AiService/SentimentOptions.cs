namespace AiService
{
    // Duygu analizi servisi yapılandırma ayarları.
    public class SentimentOptions
    {
        public string Endpoint { get; set; } = string.Empty;   // Servis BASE URL'si
        public int TimeoutSeconds { get; set; } = 8;           // İstek zaman aşımı süresi
        public string PredictPath { get; set; } = "/api/predict/"; // Gradio REST path
    }
}
