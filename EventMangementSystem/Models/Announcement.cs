using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventMangementSystem.Models
{
    public class Announcement
    {
        public int AnnouncementId { get; set; }
        public int EventId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual Event Event { get; set; }
    }

}