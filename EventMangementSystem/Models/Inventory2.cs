using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EventMangementSystem.Models
{
    public class Inventory2
    {
        public int Id { get; set; }

        [Required]
        public int ServiceProviderId { get; set; }

        [Required]
        public string EquipmentName { get; set; }

        [Required]
        public int QuantityAvailable { get; set; }

        public bool IsAvailable { get; set; }

        // Navigation property
        public virtual ServiceProvider ServiceProvider { get; set; }
    }
}