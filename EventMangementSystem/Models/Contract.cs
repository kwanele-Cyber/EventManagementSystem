// Models/Contract.cs
using System;
using System.ComponentModel.DataAnnotations;
namespace EventMangementSystem.Models
{
    public class Contract
    {
        public int Id { get; set; }
        [Required]
        public int ServiceRequestId { get; set; }
        public string TermsAndConditions { get; set; }
        [Required]
        public decimal TotalAmount { get; set; }
        public bool IsSigned { get; set; }
        public DateTime SignedDate { get; set; }
        public virtual ServiceRequest ServiceRequest { get; set; }
    }
}
