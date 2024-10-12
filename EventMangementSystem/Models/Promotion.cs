using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventMangementSystem.Models
{
    public class Promotion
    {
        public int PromotionId { get; set; }
        public int EventId { get; set; }
        public string FacebookPostId { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual Event Event { get; set; }
    }

}