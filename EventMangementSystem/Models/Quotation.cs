// Models/Quotation.cs
using System.ComponentModel.DataAnnotations;
namespace EventMangementSystem.Models
{
    public class Quotation
    {
        public int Id { get; set; }

        [Required]
        public int ServiceRequestId { get; set; }

        [Required]
        public int ServiceProviderId { get; set; }

        [Required]
        public decimal Price { get; set; }

        public string Description { get; set; }

        public bool IsApproved { get; set; }

        // Navigation properties
        public virtual ServiceRequest ServiceRequest { get; set; }
        public virtual ServiceProvider ServiceProvider { get; set; }
    }
}
