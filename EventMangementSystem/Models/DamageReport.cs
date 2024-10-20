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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReportId { get; set; }

        [Required]
        public int EquipmentId { get; set; }

        [Required]
        [StringLength(500)]
        public string DamageDescription { get; set; }

        [Required]
        public DateTime ReportDate { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        public double TotalCost { get; set; }



        // Navigation properties
        [ForeignKey("EquipmentId")]
        public virtual Inventory Inventory { get; set; }

        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }


    }
}