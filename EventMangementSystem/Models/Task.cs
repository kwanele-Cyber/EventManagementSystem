using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace EventMangementSystem.Models
{
    public class Task
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TaskId { get; set; }

        [Required]
        public string TaskName { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; }

        public int TeamId { get; set; }

        [ForeignKey("TeamId")]
        public virtual Team Team { get; set; }

        public TaskStatus Status { get; set; }

        public string Dependencies { get; set; } // Optional: Dependencies on other tasks

        public double Progress { get; set; } // Progress in percentage (0-1 range)
    }

    public enum TaskStatus
    {
        NotStarted,
        InProgress,
        Completed,
        OnHold
    }
}