using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public string SenderUserName { get; set; }

        public AppUser Sender { get; set; }
        public int RecipientId { get; set; }
        public string RecipientUserName { get; set; }
        public AppUser Recipient { get; set; }
        public string Content { get; set; }
        public DateTime? MessageRead { get; set; }
        public DateTime MessageSent { get; set; } = DateTime.UtcNow;
        public bool DeleteBySender { get; set; }
        public bool DeletedByRecipient { get; set; }
    }
}