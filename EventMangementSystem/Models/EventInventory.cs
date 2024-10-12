using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EventMangementSystem.Models
{
    public class EventInventory
    {
        public int EventInventoryId { get; set; }
        public int EventId { get; set; }

        public int InventoryId { get; set; }
        public int QuantityRequired { get; set; }
        public int UniqueCode { get; set; }
        public string DriverSignature { get; set; }
        public string AdminSignature { get; set; }
        public string Status { get; set; }
        public string Email { get; set; }
        public string QrCodePicture { get; set; }
        public string Address { get; set; }
        public string DriverEmail { get; set; }
        public string FirstName { get; set; }
        public string ManagerSignature { get; set; }
        public string PreferredTime { get; set; }
        public int DeliveredBy { get; set; }
        [Column(TypeName = "datetime2")]
        public DateTime DeliveryDate { get; set; }
        [Column(TypeName = "datetime2")]
        public DateTime DeliveredOn { get; set; }
        public bool IsDeliveryRescheduled { get; set; }

        // Updated to a collection of Inventory items
        public virtual ICollection<Inventory> Inventories { get; set; }
        public virtual Inventory Inventory { get; set; }
        public virtual Event Event { get; set; }
    }
}