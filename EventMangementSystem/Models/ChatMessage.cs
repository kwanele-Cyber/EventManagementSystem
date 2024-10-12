using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventMangementSystem.Models
{
    public class ChatMessage
    {
        public int ChatMessageId { get; set; }
        public int ChatSessionId { get; set; }
        public virtual ChatSession ChatSession { get; set; }
        public string UserId { get; set; } // Change to string to match ASP.NET Identity UserId type
        public virtual ApplicationUser User { get; set; } // Ensure this points to the correct user class
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }

    }

}