using System.Collections.Generic;
using EventMangementSystem.Models;

namespace EventMangementSystem.ViewModels
{
    public class TeamEmployeeViewModel
    {
        public Team Team { get; set; } // Holds the team data
        public List<Employee> Employees { get; set; } // List of employees for selection

        // Property to hold selected employee IDs when the form is submitted
        public List<int> SelectedEmployeeIds { get; set; }

        public TeamEmployeeViewModel()
        {
            Employees = new List<Employee>();
            SelectedEmployeeIds = new List<int>();
        }
    }
}
