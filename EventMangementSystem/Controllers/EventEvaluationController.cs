using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EventMangementSystem.Models;

namespace EventMangementSystem.Controllers
{
    
    

    public class EventEvaluationController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: EventEvaluation/Create
        public ActionResult Create(int eventId)
        {
            var ticket = db.Tickets.Where(x=>x.AttendeeEmail == User.Identity.Name).FirstOrDefault();
            var eventEvaluation = new EventEvaluation { EventId = eventId,AttendeeName = ticket.AttendeeName };
            return View(eventEvaluation);
        }

        // POST: EventEvaluation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EventEvaluation eventEvaluation)
        {
            if (ModelState.IsValid)
            {
                db.EventEvaluations.Add(eventEvaluation);
                db.SaveChanges();
                TempData["SuccessMessage"] = "We have recived your feedback Thank you";

                return RedirectToAction("Index","Home");
            }

            return View(eventEvaluation);
        }

        // GET: EventEvaluation/Index
        public ActionResult Index(int eventId)
        {
            var evaluations = db.EventEvaluations.Where(ee => ee.EventId == eventId).ToList();
            return View(evaluations);
        }
    }

}