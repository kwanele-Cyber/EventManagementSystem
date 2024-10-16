// Models/ServiceRequest.cs
using EventMangementSystem.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace EventMangementSystem.Models
{
    public class ServiceRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public int EventId { get; set; }
        public string ServiceName { get; set; }
        [Required]
        public string ServiceCategory { get; set; }  // e.g., Catering, Lighting, Audio
        public string Priority { get; set; } // Urgent, Standard, Low
        public string EventManagerSignature { get; set; } // Urgent, Standard, Low
        public string ServiceProviderSignature { get; set; } // Urgent, Standard, Low
        public bool IsAssigned { get; set; }
        public int? ServiceProviderId { get; set; }
        public bool IsOpenForBidding { get; set; }
        public bool IsCompleted { get; set; }
        public virtual Event Event { get; set; }
        public virtual ServiceProvider ServiceProvider { get; set; }
        public virtual ICollection<Quotation> Bids { get; set; }


        public ServiceRequestStatus Status { get; set; }


        public string StartCode { get; set; } // A four-digit code to start the service
        public string FinishCode { get; set; } // The QR code used to finish the service

    }

    public enum ServiceRequestStatus
    {
        Open,
        Assigned,
        InProgress,
        Completed,
        Cancelled
    }
}
