using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventMangementSystem.Models
{
    public class Ticket
    {
        public int TicketId { get; set; }
        public int EventId { get; set; }
        public string AttendeeName { get; set; }
        public string AttendeeEmail { get; set; }
        public int Quantity { get; set; }
        public int tempqty { get; set; }
        public decimal TotalPrice { get; set; }
        public string CheckInCode { get; set; }
        public string QRCode { get; set; }
        public string ChargeID { get; set; }
        public bool IsCheckedIn { get; set; }
        public bool Refunded { get; set; }
        public virtual Event Event { get; set; }
    }




}