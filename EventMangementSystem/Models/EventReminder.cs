using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EventMangementSystem.Models
{
    public class EventReminder
    {
        public int EventReminderId { get; set; }
        public int EventId { get; set; }
        public string UserEmail { get; set; }
        [DataType(DataType.Time)]
        public DateTime ReminderTime { get; set; }
        public bool IsSent { get; set; }
        public virtual Event Event { get; set; }
    }

}