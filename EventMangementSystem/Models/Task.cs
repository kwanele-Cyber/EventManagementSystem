using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace EventMangementSystem.Models
{
    public class GroupTask
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

        public DateTime? ActualEndTime { get; set; }

        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; }

        public int TeamId { get; set; }

        [ForeignKey("TeamId")]
        public virtual Team Team { get; set; }

        public GroupTaskStatus Status { get; set; }

        public string Dependencies { get; set; } // Optional: Dependencies on other tasks

        public double Progress { get; set; } // Progress in percentage (0-100 range)

    }

    public class TaskTime
    {
        [Range(0, 23)]  // Hours are from 0 to 23
        public int Hour { get; set; }

        [Range(0, 59)]  // Minutes are from 0 to 59
        public int Minute { get; set; }

        [Range(0, 59)]  // Seconds are from 0 to 59
        public int Second { get; set; }

        [Range(0, 999)]  // Milliseconds are from 0 to 999
        public int Millisecond { get; set; }

        [Range(1, 31)]  // Days are from 1 to 31 (depending on the month and leap years)
        public int Day { get; set; }

        [Range(1, 12)]  // Months are from 1 to 12
        public int Month { get; set; }

        [Range(1, 9999)]  // Years are from 1 to 9999 (as in DateTime)
        public int Year { get; set; }

        public TaskTime()
        { }

        // Constructor for manual initialization
        public TaskTime(int hour, int minute, int second, int millisecond, int day, int month, int year)
        {
            Hour = hour;
            Minute = minute;
            Second = second;
            Millisecond = millisecond;
            Day = day;
            Month = month;
            Year = year;
        }

        // Constructor for initialization from DateTime
        public TaskTime(DateTime time)
        {
            Hour = time.Hour;
            Minute = time.Minute;
            Second = time.Second;
            Millisecond = time.Millisecond;
            Day = time.Day;
            Month = time.Month;
            Year = time.Year;
        }

        // Method to convert TaskTime back to DateTime
        public DateTime ToDateTime()
        {
            return new DateTime(Year, Month, Day, Hour, Minute, Second, Millisecond);
        }
    }

    public class GroupTaskViewModel
    {
        public string TaskId { get; set; }
        public string TaskName { get; set; }
        public TaskTime StartDate { get; set; }
        public TaskTime EndDate { get; set; }
        public string Dependencies { get; set; }
        public double Progress { get; set; }
        public Employee AssignedTo { get; set; }
    }

    public enum GroupTaskStatus
    {
        NotStarted,
        InProgress,
        Completed,
        OnHold
    }
}