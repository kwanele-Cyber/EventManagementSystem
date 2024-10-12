using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventMangementSystem.Models
{
    public class Inventory
    {
        public int InventoryId { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public string picture { get; set; }
        public int QuantityAvailable { get; set; }
    }

}