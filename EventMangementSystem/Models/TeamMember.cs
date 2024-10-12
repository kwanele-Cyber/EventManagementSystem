using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventMangementSystem.Models
{
    public class TeamMember
    {
        [Key]
        public int TeamMemberId { get; set; }

        // Foreign key to Employee entity
        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey(nameof(EmployeeId))]
        public virtual Employee Employee { get; set; }

        // Foreign key to Team entity
        [Required]
        
        public int TeamId { get; set; }

        [ForeignKey(nameof(TeamId))]
        public virtual Team Team { get; set; }

        // Role of the employee within the team (e.g., Leader, Technician)
        public string Role { get; set; }
    }
}
