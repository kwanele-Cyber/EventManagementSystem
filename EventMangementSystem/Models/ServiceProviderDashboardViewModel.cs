
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventMangementSystem.Models
{
    public class ServiceProviderDashboardViewModel
    {
        public decimal TotalEarnings { get; set; }
        public int UpcomingBookingsCount { get; set; }
        public List<UpcomingBooking> UpcomingBookings { get; set; }
        public List<string> RevenueLabels { get; set; }
        public List<decimal> RevenueData { get; set; }
    }
    public class UpcomingBooking
    {
        public string EventName { get; set; }
        public DateTime EventDateTime { get; set; }
        public string Status { get; set; }
        public int Id { get; set; }
        public int ServiceProviderId { get; set; }
    }

}
