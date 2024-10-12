using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;

using SendGrid;
using SendGrid.Helpers.Mail;
using System.Data.Entity;
using System.Net.Mail;
using System.Net.Sockets;
namespace EventMangementSystem.Models
{
    

    public class ReminderService
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public void ScheduleReminders()
        {
            RecurringJob.AddOrUpdate(() => SendReminders(), Cron.Minutely);
        }

        public async Task SendReminders()
        {
            var reminders = db.EventReminders.Include("Event").Where(r => r.ReminderTime <= DateTime.Now && !r.IsSent).ToList();
            if(reminders.Count > 0)
            {
                foreach (var reminder in reminders)
                {
                    await SendEmailReminder(reminder);
                    reminder.IsSent = true;
                    db.Entry(reminder).State = EntityState.Modified;
                }

                db.SaveChanges();
            }
           
        }

        private async Task SendEmailReminder(EventReminder reminder)
        {
           
            string rem = $"Reminder: The event '{reminder.Event.Name}' is scheduled for {reminder.Event.Start.ToShortTimeString()} at the {reminder.Event.Date.ToLongDateString()} .";
            
            var tickes = db.Tickets.Where(x => x.EventId == reminder.EventId).ToList();
            foreach (var tick in tickes)
            {
                try
                {
                    // Prepare email message
                    var email2 = new MailMessage();
                    email2.From = new MailAddress("DbnEventMangement@outlook.com");
                    email2.To.Add(tick.AttendeeEmail);
                    email2.Subject = "Event Reminder ";
                    string emailBody = rem+"\n\n" +

                   $"Regards,\r\nEvent Mangement";
                    email2.Body = emailBody;


                    var smtpClient = new SmtpClient();

                    smtpClient.Send(email2);

                }
                catch (Exception ex)
                {
                    reminder.IsSent=false;
                    db.Entry(reminder).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }
    }

}