using System.Collections.Generic;

namespace Api.DTOs
{
    // Mesaj listesini ve toplam sayıyı içeren cevap modeli.
    public class ListMessagesResponse
    {
        public IList<MessageItemDto> Items { get; set; } = new List<MessageItemDto>(); // Mesajlar
        public int Count { get; set; }                                                 // Toplam kayıt sayısı
    }
}
