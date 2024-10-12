using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventMangementSystem.Models
{
    public class Donation
    {
        public int DonationId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal Amount { get; set; }
        public int EventId { get; set; }
        public virtual Event Event { get; set; }

    }
}