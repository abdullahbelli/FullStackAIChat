using System.Collections.Generic;

namespace Api.DTOs
{
    public class ListMessagesResponse
    {
        public IList<MessageItemDto> Items { get; set; } = new List<MessageItemDto>();
        public int Count { get; set; }
        
    }
}
