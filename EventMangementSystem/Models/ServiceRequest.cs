using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EventMangementSystem.Models
{
    public class ServiceRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int EventId { get; set; }

        [NotMapped]
        private string _name;

        public string ServiceName 
        {
            set => _name = value;
            get => ((string.IsNullOrEmpty(_name) || string.IsNullOrWhiteSpace(_name))? ServiceCategory : _name);
        }

        [Required]
        public string ServiceCategory { get; set; }  // e.g., Catering, Lighting, Audio

        public string Priority { get; set; } // Urgent, Standard, Low

        public string EventManagerSignature { get; set; }

        public string ServiceProviderSignature { get; set; }

        public bool IsAssigned { get; set; }

        public int? ServiceProviderId { get; set; } // Nullable because it might be unassigned initially
        [ForeignKey(nameof(ServiceProviderId))]
        public virtual ServiceProvider ServiceProvider { get; set; }

        public bool IsOpenForBidding { get; set; }

        public bool IsCompleted { get; set; }

        public virtual Event Event { get; set; }

        public virtual ICollection<Quotation> Bids { get; set; }

        public ServiceRequestStatus Status { get; set; }

        public string StartCode { get; set; } // A four-digit code to start the service
        public string FinishCode { get; set; } // The QR code used to finish the service

        // Foreign key for the team assigned to this service request
        public int? TeamId { get; set; } // Nullable because it may not have a team initially
        [ForeignKey(nameof(TeamId))]
        public virtual Team Team { get; set; }

        // List of tasks associated with this service request
        public virtual ICollection<GroupTask> GroupTasks { get; set; }

        // New property: Whether a team has been assigned to handle the service request
        public bool IsTeamAssigned { get; set; } = false; // New boolean to track team assignment

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
