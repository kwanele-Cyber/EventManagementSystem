using EventMangementSystem.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;


namespace EventMangementSystem.ViewModels
{
    public class TaskViewModel
    {
        public int TeamId { get; set; }

        [Required]
        public string TaskName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public int EmployeeId { get; set; } // The employee assigned to this task

        public List<Employee> TeamMembers { get; set; } // List of team members to assign the task
        public List<string> Dependencies { get; set; }
    }
}