using Api.DTOs;

namespace Api.Services
{
    public interface IMessageService
    {
        Task<CreateMessageResponse> CreateAsync(string text, CancellationToken ct = default);
        Task<ListMessagesResponse> ListAsync(int take = 50, int skip = 0, CancellationToken ct = default);
    }
}
