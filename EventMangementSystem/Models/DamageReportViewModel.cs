using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EventMangementSystem.Models
{
    public class DamageReportViewModel
    {


        public int ReportId { get; set; }


        public int EquipmentId { get; set; }


        public string DamageDescription { get; set; }


        public DateTime ReportDate { get; set; }


        public int EventId { get; set; }


        public double TotalCost { get; set; }





        public double bareCost { get; set; }
        public double vat { get; set; }





        // Navigation properties
        [ForeignKey("EquipmentId")]
        public virtual Inventory Inventory { get; set; }

        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }



    }
}