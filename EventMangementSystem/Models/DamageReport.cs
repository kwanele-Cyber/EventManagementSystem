using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace EventMangementSystem.Models
{

    public enum DamageType
    {
        Minor,
        Moderate,
        Severe,
        Missing
    }
    public class DamageReport
    {
        [Key]
        public int ReportId { get; set; }


        public string DamageDescription { get; set; }

        [Required]
        public DateTime ReportDate { get; set; }

        [Required]
        public int EventId { get; set; }

        public double TotalCost { get; set; }

        public int findRecord { get; set; }

        //NEWLY
        [Required]
        public int EquipmentId { get; set; }

        // Navigation properties
        [ForeignKey("EquipmentId")]
        public virtual Inventory Inventory { get; set; }

        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }

        [ForeignKey("findRecord")]
        public virtual ReturnProcess ReturnProcess { get; set; }

    }
}