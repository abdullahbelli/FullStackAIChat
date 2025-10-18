using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AiService  // <-- tam olarak bu namespace
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSentimentService(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<SentimentOptions>(configuration.GetSection("Sentiment"));
            services.AddHttpClient<ISentimentClient, SentimentClient>();
            return services;
        }
    }
}
