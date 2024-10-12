using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventMangementSystem.Models
{
    public class Team
    {
        [Key]
        public int TeamId { get; set; }

        [Required]
        public string TeamName { get; set; }

        public string Description { get; set; }

        // Navigation property for employees who are members of the team
        public virtual ICollection<TeamMember> TeamMembers { get; set; }

        // Optional: You can associate the team with a ServiceProvider
        public int? ServiceProviderId { get; set; }
        [ForeignKey(nameof(ServiceProviderId))]
        public virtual ServiceProvider ServiceProvider { get; set; }

        // Add this property
        public virtual ICollection<GroupTask> GroupTasks { get; set; }
    }
}
