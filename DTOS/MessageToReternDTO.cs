using System;

namespace DatingApp.API.DTOS
{
    public class MessageToReternDTO
    {        
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int RecipientId { get; set; }
        public string SenderPhotoUrl { get; set; }
        public string RecipientPhotoUrl { get; set; }
        public string SenderName { get; set; }
        public string RecipientName { get; set; }
        public string Content { get; set; }
        public bool IsRead { get; set; }
        public DateTime? DateRead { get; set; }
        public DateTime MessageSent { get; set; }
    }
}