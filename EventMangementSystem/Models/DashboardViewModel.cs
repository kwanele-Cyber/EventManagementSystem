using System;
using System.Collections.Generic;

namespace YourNamespace.Models
{
    public class DashboardViewModel
    {
        public int TotalEvents { get; set; }
        public int TotalServiceRequests { get; set; }
        public int TotalQuotations { get; set; }
        public decimal AverageQuotationAmount { get; set; }

        public List<EventSummary> RecentEvents { get; set; }
        public List<ServiceRequestSummary> RecentServiceRequests { get; set; }
        public List<QuotationSummary> RecentQuotations { get; set; }
    }

    public class EventSummary
    {
        public int EventId { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public int NumberOfServiceRequests { get; set; }
    }

    public class ServiceRequestSummary
    {
        public int ServiceRequestId { get; set; }
        public string ServiceType { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
    }

    public class QuotationSummary
    {
        public int QuotationId { get; set; }
        public int ServiceRequestId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
