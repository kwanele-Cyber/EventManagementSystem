using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace EventMangementSystem.Models
{
    public class Event
    {
        public int EventId { get; set; }
        public string Name { get; set; }
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        [DataType(DataType.Time)]
        public DateTime Start { get; set; }
        [DataType(DataType.Time)]
        public DateTime End { get; set; }
        public string EventMangerEmail { get; set; }
        public string Location { get; set; }
        public string status { get; set; }
        public string Description { get; set; }
        public decimal TicketPrice { get; set; }
        public bool Canceled { get; set; } // New property
        public string PicturePath { get; set; } 
        public int VenueId { get; set; }
        public virtual Venue Venue { get; set; }
        public virtual ICollection<EventInventory> EventInventories { get; set; } = new List<EventInventory>();
        public virtual ICollection<ServiceRequest> ServiceRequests { get; set; }
    }
    




}