namespace AiService
{
    public class SentimentOptions
    {
        public string Endpoint { get; set; } = string.Empty;   // Base URL of the Space
        public int TimeoutSeconds { get; set; } = 8;
        public string PredictPath { get; set; } = "/api/predict/"; // Gradio REST path
    }
}
