using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventMangementSystem.Models
{
    public class EventAnalyticsViewModel
    {
        public Event Event { get; set; }
        public int TotalTicketsSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalTicketIncome { get; set; }
        public double AverageRating { get; set; }
        public decimal TotalDonations { get; set; }
        public int TotalFeedbackCount { get; set; }
        public int TotalAttendees { get; set; }
    }

}
