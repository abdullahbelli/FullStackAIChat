using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AiService
{
    // Duygu analizi servisini DI konteynerine ekler
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSentimentService(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Ayarları yükler ve HTTP istemcisini kaydeder
            services.Configure<SentimentOptions>(configuration.GetSection("Sentiment"));
            services.AddHttpClient<ISentimentClient, SentimentClient>();
            return services;
        }
    }
}
