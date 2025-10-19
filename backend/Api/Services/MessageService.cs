using Api.Data;
using Api.DTOs;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using AiService;

namespace Api.Services
{
    // Mesaj oluşturma ve listeleme işlemlerini yönetir
    internal sealed class MessageService : IMessageService
    {
        private readonly AppDbContext _db;
        private readonly ISentimentClient _sentiment;

        public MessageService(AppDbContext db, ISentimentClient sentiment)
        {
            _db = db;
            _sentiment = sentiment;
        }

        // Yeni mesaj oluşturur ve duygu analizini uygular
        public async Task<CreateMessageResponse> CreateAsync(string text, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Text is required.", nameof(text));

            var input = text.Trim();
            if (input.Length > 1000) input = input[..1000];

            var utcNow = DateTime.UtcNow;
            var label = "neutral";
            var score = 0.0;

            try
            {
                (label, score) = await _sentiment.AnalyzeAsync(input, ct);
            }
            catch (Exception ex)
            {
                // Analiz başarısızsa nötr değerler
                Console.WriteLine("[Sentiment] fallback neutral. Reason: " + ex.Message);
                label = "neutral";
                score = 0.0;
            }

            var entity = new Message
            {
                Text = input,
                Sentiment = label,
                Score = score,
                CreatedAt = utcNow
            };

            _db.Messages.Add(entity);
            await _db.SaveChangesAsync(ct);

            return new CreateMessageResponse
            {
                Id = entity.Id,
                Text = entity.Text,
                Sentiment = entity.Sentiment,
                Score = entity.Score,
                CreatedAt = entity.CreatedAt
            };
        }

        // Mesaj listesini zaman sırasına göre döndür
        public async Task<ListMessagesResponse> ListAsync(int take = 50, int skip = 0, CancellationToken ct = default)
        {
            take = Math.Clamp(take, 1, 200);
            skip = Math.Max(0, skip);

            var query = _db.Messages
                           .AsNoTracking()
                           .OrderByDescending(m => m.CreatedAt);

            var items = await query.Skip(skip).Take(take)
                .Select(m => new MessageItemDto
                {
                    Id = m.Id,
                    Text = m.Text,
                    Sentiment = m.Sentiment,
                    Score = m.Score,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync(ct);

            return new ListMessagesResponse
            {
                Items = items,
                Count = items.Count
            };
        }
    }
}
