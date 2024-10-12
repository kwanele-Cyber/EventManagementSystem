using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EventMangementSystem.Models;

namespace EventMangementSystem.Controllers
{
    
    public class AnnouncementController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Announcement/Index/5
        public ActionResult Index(int eventId)
        {
            if (!User.IsInRole("EventManager"))
            {


                // Ensure the user is an attendee with a valid ticket for the event
                var userEmail = User.Identity.Name; // Assuming the user is authenticated and their email is their identity name
                var hasTicket = db.Tickets.Any(t => t.EventId == eventId && t.AttendeeEmail == userEmail);

                if (!hasTicket)
                {
                    return new HttpStatusCodeResult(403, "You are not authorized to view these announcements.");
                }
            }
            var announcements = db.Announcements.Where(a => a.EventId == eventId).ToList();
            ViewBag.EventName = db.Events.Find(eventId)?.Name;
            return View(announcements);
        }

        // GET: Announcement/Create/5
        public ActionResult Create(int eventId)
        {
            var announcement = new Announcement { EventId = eventId };
            return View(announcement);
        }

        // POST: Announcement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Announcement announcement)
        {
            if (ModelState.IsValid)
            {
                announcement.CreatedAt = DateTime.Now;
                db.Announcements.Add(announcement);
                db.SaveChanges();
                return RedirectToAction("Index", new { eventId = announcement.EventId });
            }

            return View(announcement);
        }
    }

}