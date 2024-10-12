using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EventMangementSystem.Models;
using System.Data.Entity;

namespace EventMangementSystem.Controllers
{
    

    public class CheckInController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: CheckIn
        public ActionResult Index()
        {
            return View();
        }

        // POST: CheckIn/Scan
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Scan(string code)
        {
            
            var ticket = db.Tickets.FirstOrDefault(t => t.CheckInCode == code);
           
           
            if (ticket == null || ticket.IsCheckedIn)
            {
                TempData["ErrorMessage"] = "Invalid or already checked-in ticket.";
                return RedirectToAction("Index");
            }
            var events = db.Events.FirstOrDefault(t => t.EventId == ticket.EventId);
            events.status = "Started";
            //if (ticket.Quantity > 1)
            //{
            //    ticket.Quantity -= 1;
                
            //    db.Entry(ticket).State = EntityState.Modified; ;
            //    db.SaveChanges();
            //    TempData["SuccessMessage"] = "Check-in successful for " + ticket.AttendeeName + " quests remaining " + ticket.Quantity;
            //    return RedirectToAction("Index");

            //}

            ticket.IsCheckedIn = true;
            db.SaveChanges();

            TempData["SuccessMessage"] = "Check-in successful for " + ticket.AttendeeName+ " Number of Quests: "+ ticket.tempqty;
            return RedirectToAction("Index");
        }
    }

}