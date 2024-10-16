using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using QRCoder;
using EventMangementSystem.Models;
using Stripe;
using System.Data.Entity;
using System.Web;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net.Mail;
using PayPal.Api;
using Task = System.Threading.Tasks.Task;


namespace EventMangementSystem.Controllers
{
    
    public class EventController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private StripeService stripeService = new StripeService();
        // GET: Event
        public ActionResult Index()
        {
            var events = db.Events.Include(e => e.Venue).Include(e => e.EventInventories).Where(x=>x.Canceled == false).ToList();
            return View(events);
        }
        // GET: Event/Details/5
        public ActionResult Details(int id)
        {
            var @event = db.Events
                .Include(e => e.Venue)
                .Include(e => e.EventInventories)
                .FirstOrDefault(e => e.EventId == id);

            if (@event == null)
            {
                return HttpNotFound();
            }

            return View(@event);
        }
        // GET: Event/Create
        public ActionResult Create()
        {
            ViewBag.VenueId = new SelectList(db.Venues, "VenueId", "Name");
            return View(new EventMangementSystem.Models.Event());
        }

        // POST: Event/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "EventId,Name,Date,Start,End,Location,Description,TicketPrice,VenueId")] EventMangementSystem.Models.Event @event, HttpPostedFileBase EventPicture)
        {
            if (ModelState.IsValid)
            {
                if (EventPicture != null && EventPicture.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(EventPicture.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/"), fileName);
                    EventPicture.SaveAs(path);
                    @event.PicturePath = "/Content/" + fileName;
                }
                @event.EventMangerEmail = User.Identity.Name;
                db.Events.Add(@event);
                db.SaveChanges();

                // Redirect to add inventories after event creation
                return RedirectToAction("Add", "Inventory", new { eventId = @event.EventId });
            }

            ViewBag.VenueId = new SelectList(db.Venues, "VenueId", "Name", @event.VenueId);
            return View(@event);
        }




        // GET: Event/Create
        //// GET: Event/Create
        //public ActionResult Create()
        //{
        //    ViewBag.VenueId = new SelectList(db.Venues, "VenueId", "Name");
        //    ViewBag.Inventories = db.Inventories.ToList();
        //    return View(new EventMangementSystem.Models.Event { EventInventories = new List<EventInventory>() });
        //}

        //// POST: Event/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "EventId,Name,Date,Start,End,Location,Description,TicketPrice,VenueId")] EventMangementSystem.Models.Event @event, HttpPostedFileBase EventPicture, List<EventInventory> Inventories)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        if (EventPicture != null && EventPicture.ContentLength > 0)
        //        {
        //            var fileName = Path.GetFileName(EventPicture.FileName);
        //            var path = Path.Combine(Server.MapPath("~/Content/"), fileName);
        //            EventPicture.SaveAs(path);
        //            @event.PicturePath = "/Content/" + fileName;
        //        }
        //        @event.EventMangerEmail = User.Identity.Name;
        //        // Save the event first
        //        db.Events.Add(@event);
        //        db.SaveChanges();

        //        // Get the event ID of the newly created event
        //        var eventId = @event.EventId;

        //        // Create and save EventInventory records
        //        foreach (var inventory in Inventories)
        //        {
        //            if (inventory.InventoryId != 0 && inventory.QuantityRequired > 0)
        //            {
        //                db.EventInventories.Add(new EventInventory
        //                {
        //                    EventId = eventId,
        //                    InventoryId = inventory.InventoryId,
        //                    QuantityRequired = inventory.QuantityRequired,
        //                    Address = @event.Location,
        //                    Status = "Not-Assigned",
        //                    Email = @event.EventMangerEmail

        //                });
        //            }
        //        }

        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    ViewBag.VenueId = new SelectList(db.Venues, "VenueId", "Name", @event.VenueId);
        //    ViewBag.Inventories = db.Inventories.ToList();
        //    return View(@event);
        //}



        public ActionResult Analytics(int id)
        {
            var @event = db.Events
                .Include(e => e.Venue)
                .FirstOrDefault(e => e.EventId == id);

            if (@event == null)
            {
                return HttpNotFound();
            }

            var totalTicketsSold = db.Tickets.Where(t => t.EventId == id).Count();
           
            var totalDonationIncome = db.Donations.Where(t => t.EventId == id).Sum(t => (decimal?)t.Amount) ?? 0;
            var totalTicketIncome = db.Tickets.Where(t => t.EventId == id).Sum(t => (decimal?)t.TotalPrice) ?? 0;
            var averageRating = db.EventEvaluations.Where(e => e.EventId == id).Average(e => (double?)e.Rating) ?? 0;
            var totalFeedbackCount = db.EventEvaluations.Where(e => e.EventId == id).Count();
            var totalAttendees = db.Tickets.Where(t => t.EventId == id && t.IsCheckedIn).Count();

            var analytics = new EventAnalyticsViewModel
            {
                Event = @event,
                TotalDonations = totalDonationIncome,
                TotalTicketsSold = totalTicketsSold,
                TotalTicketIncome = totalTicketIncome,
                TotalRevenue = totalTicketIncome + totalDonationIncome,
                
                
                AverageRating = averageRating,
                TotalFeedbackCount = totalFeedbackCount,
                TotalAttendees = totalAttendees
            };

            return View(analytics);
        }



        public ActionResult Search(string searchTerm)
        {
            var events = db.Events
                .Include(e => e.Venue)
                .Where(e => e.Name.Contains(searchTerm) || e.Location.Contains(searchTerm))
                .ToList();

            return View(events);
        }

        // GET: Event/Cancel/5
        public ActionResult Cancel(int id)
        {
            var @event = db.Events.Find(id);
            if (@event == null)
            {
                return HttpNotFound();
            }

            return View(@event);
        }

        // POST: Event/Cancel/5
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CancelConfirmed(int id)
        {
            var @event = db.Events.Find(id);
            if (@event == null)
            {
                return HttpNotFound();
            }

            @event.Canceled = true;
            db.Entry(@event).State = EntityState.Modified;
            db.SaveChanges();

            await NotifyAttendees(@event);

            var tickets = db.Tickets.Where(t => t.EventId == @event.EventId).ToList();
            
            return RedirectToAction("PayAttendees", new { id = id });

        }
        public ActionResult PayAttendees(int id)
        {
            var tickets = db.Tickets.Where(t => t.EventId == id && t.Refunded == false).FirstOrDefault();
            
            if(tickets!=null)
            {
                return RedirectToAction("CreatePayment", "PayPal", new { CartTotal = tickets.TotalPrice,id= tickets.TicketId, refund="Yes" });
            }
            else
            {

                TempData["SuccessMessage"] = "All attendees ware refunded Successfully";

            }

            return RedirectToAction("Details", new { id = id });
            
        }

        private async Task NotifyAttendees(EventMangementSystem.Models.Event @event)
        {
            var attendees = db.Tickets.Where(t => t.EventId == @event.EventId).Select(t => t.AttendeeEmail).ToList();

            foreach (var email in attendees)
            {
                string subject = "Event Cancellation: " + @event.Name;
                string message = $"Dear Attendee,\n\nWe regret to inform you that the event '{@event.Name}' scheduled for {@event.Date} at {@event.Location} has been canceled. We apologize for any inconvenience this may cause.\n\nSincerely,\nEvent Management Team";
                try
                {
                    // Prepare email message
                    var email2 = new MailMessage();
                    email2.From = new MailAddress("DbnEventMangement@outlook.com");
                    email2.To.Add(email);
                    email2.Subject = subject;
                    string emailBody= message;
                    email2.Body = emailBody;


                    var smtpClient = new SmtpClient();

                    smtpClient.Send(email2);
                    TempData["SuccessMessage"] = "Event cancelled Successfully";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Failed to send Email due to" + ex.Message;
                }
            }
        }

        /
        // GET: Request multiple services
        public ActionResult RequestService()
        {
            // Make sure both Id and Name properties are selected
            ViewBag.Events = db.Events.Select(e => new { Id = e.EventId, Name = e.Name }).ToList();
            ViewBag.ServiceCategories = db.ServiceCategories.Select(c => new { Id = c.Id, Name = c.Name }).ToList();

            // Initialize with one empty service request
            var requests = new List<ServiceRequest> { new ServiceRequest() };

            return View(requests);
        }


        // POST: Submit multiple service requests
        [HttpPost]
        public ActionResult RequestService(List<ServiceRequest> requests)
        {
            if (ModelState.IsValid)
            {
                foreach (var request in requests)
                {

                    db.ServiceRequests.Add(request);
                }

                db.SaveChanges();
                return RedirectToAction("ServiceRequests");
            }
            // If model validation fails, return to the view with the existing data
            ViewBag.Events = db.Events.Select(e => new { e.EventId, e.Name }).ToList();
            ViewBag.ServiceCategories = db.ServiceCategories.Select(c => new { c.Id, c.Name }).ToList();
            return View(requests);
        }



        // GET: Event/Purchase/5
        public ActionResult Purchase(int id)
        {
            var @event = db.Events.Include(e => e.Venue).Include(e => e.EventInventories).FirstOrDefault(e => e.EventId == id);

            if (@event == null)
            {
                return HttpNotFound();
            }

            var ticket = new Ticket { EventId = @event.EventId, Event = @event, AttendeeEmail=User.Identity.Name };
            return View(ticket);
        }

        // POST: Event/Purchase
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Purchase(Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                var @event = db.Events.Find(ticket.EventId);
                if (@event == null)
                {
                    return HttpNotFound();
                }

                ticket.TotalPrice = ticket.Quantity * @event.TicketPrice;
                ticket.CheckInCode = Guid.NewGuid().ToString();
                ticket.tempqty = ticket.Quantity;
                db.Tickets.Add(ticket);

                db.SaveChanges();

                return RedirectToAction("CreatePayment", "PayPal", new { CartTotal = ticket.TotalPrice, id = ticket.TicketId });
                
            }

            return View(ticket);
        }

        private async Task<Stripe.Charge> ProcessPayment(decimal amount, string email)
        {
            var options = new ChargeCreateOptions
            {
                Amount = (long)(amount * 100), // Amount in cents
                Currency = "usd",
                Description = "Ticket Purchase",
                ReceiptEmail = email,
                Source = "tok_visa" // Use a test token or collect the token from your frontend
            };

            var service = new ChargeService();
            try
            {
                var charge = await service.CreateAsync(options);
                return charge;
            }
            catch (StripeException ex)
            {
                // Handle payment failure
                return null;
            }
        }

        public ActionResult Success()
        {
            return View();
        }
    }

    


}