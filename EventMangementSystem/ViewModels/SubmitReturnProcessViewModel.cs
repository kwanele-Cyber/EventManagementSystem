using System.Collections.Generic;

namespace EventMangementSystem.ViewModels
{
    public class SubmitReturnProcessViewModel
    {
        public int AssignmentId { get; set; }
        public Dictionary<int, int> QuantityReturned { get; set; }
        public Dictionary<int, string> InspectionCondition { get; set; }
        public Dictionary<int, string> InspectionNotes { get; set; }
        public Dictionary<int, decimal> RepairCost { get; set; }
        public Dictionary<int, decimal> MissingItemCost { get; set; }
    }
}