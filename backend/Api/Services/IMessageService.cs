using Api.DTOs;

namespace Api.Services
{
    // Mesaj işlemleri için servis arayüzü.
    public interface IMessageService
    {
        Task<CreateMessageResponse> CreateAsync(string text, CancellationToken ct = default); // Yeni mesaj oluşturur
        Task<ListMessagesResponse> ListAsync(int take = 50, int skip = 0, CancellationToken ct = default); // Mesajları listeler
    }
}
