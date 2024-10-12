using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventMangementSystem.Models
{
    public class ChatSession
    {
        public int ChatSessionId { get; set; }
        public int EventId { get; set; }
        public virtual Event Event { get; set; }
        public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    }
}