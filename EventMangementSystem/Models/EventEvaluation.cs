using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventMangementSystem.Models
{
    public class EventEvaluation
    {
        public int EventEvaluationId { get; set; }
        public int EventId { get; set; }
        public string AttendeeName { get; set; }
        public string Feedback { get; set; }
        public int Rating { get; set; } // Rating out of 5

        public virtual Event Event { get; set; }
    }

}