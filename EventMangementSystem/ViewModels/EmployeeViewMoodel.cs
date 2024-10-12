using System;

namespace EventMangementSystem.ViewModels
{
    public class EmployeeViewModel
    {
        // ApplicationUser Fields
        public string Name { get; set; } // Full name of the employee
        public string Email { get; set; } // Employee's email (used as Username in Identity)
        public string Password { get; set; } // Password for the new identity user
        public string Role { get; set; } // Role (e.g., "Employee", "Manager", etc.)

        // Employee-specific fields
        public string Position { get; set; } // Job Position, e.g., "Technician", "Manager"
        public DateTime DateHired { get; set; } = DateTime.Today; // Hire Date
    }
}
