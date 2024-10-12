using EventMangementSystem.Models;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;



namespace EventMangementSystem.Controllers
{
    
    public class EventReminderController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: EventReminder/Create
        public ActionResult Create(int eventId)
        {
            var eventReminder = new EventReminder { EventId = eventId, UserEmail = User.Identity.Name };
            return View(eventReminder);
        }

        // POST: EventReminder/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "EventReminderId,EventId,UserEmail,ReminderTime")] EventReminder eventReminder)
        {
            if (ModelState.IsValid)
            {
                db.EventReminders.Add(eventReminder);
                db.SaveChanges();
                return RedirectToAction("Details", "Event", new { id = eventReminder.EventId });
            }

            return View(eventReminder);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

}