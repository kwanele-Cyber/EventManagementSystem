using EventMangementSystem.Models;
using System.Collections.Generic;

namespace EventMangementSystem.ViewModels
{
    public class PerformServiceViewModel
    {
        public ServiceRequest ServiceRequest { get; set; }
        public List<GroupTask> Tasks { get; set; }
        public string ReturnUrl { get; set; }
        public int BidId { get; set; }
    }

}