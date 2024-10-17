using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EventMangementSystem.Models
{
    public class ReturnProcess
    {
        [Key]
        public int ReturnProcessId { get; set; }

        // Reference to the DriverAssignment
        public int DriverAssignmentId { get; set; }
        [ForeignKey("DriverAssignmentId")]
        public virtual DriverAssignment DriverAssignment { get; set; }

        // Reference to the EventInventory
        public int EventInventoryId { get; set; }
        [ForeignKey("EventInventoryId")]
        public virtual EventInventory EventInventory { get; set; }

        // Store the quantity returned during the return process
        public int QuantityReturned { get; set; }

        // Store the status after the return process
        public string Status { get; set; }

        // Timestamp for when the return was submitted
        [Column(TypeName = "datetime2")]
        public DateTime ReturnSubmittedOn { get; set; }

        // Additional fields for inspection details
        public string InspectionCondition { get; set; } // Condition of the item after return
        public string InspectionNotes { get; set; } // Additional notes from inspection
        public decimal RepairCost { get; set; } // Cost for repairs, if needed
        public decimal MissingItemCost { get; set; } // Cost for missing items

        // Inspection completion timestamp
        [Column(TypeName = "datetime2")]
        public DateTime? InspectionCompletedOn { get; set; }
    }
}