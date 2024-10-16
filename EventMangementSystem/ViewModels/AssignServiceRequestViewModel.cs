using EventMangementSystem.Models;
using System.Collections.Generic;

namespace EventMangementSystem.ViewModels
{
    public class AssignServiceRequestViewModel
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public string ReturnUrl { get; set; }

        // List of available service requests (not causing time conflicts)
        public IEnumerable<ServiceRequest> AvailableServiceRequests { get; set; }
    }

}
