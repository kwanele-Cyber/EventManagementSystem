using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EventMangementSystem.Models
{

    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        [StringLength(255)]
        public string Message { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } // Assuming you are using ASP.NET Identity's ApplicationUser
    }

}