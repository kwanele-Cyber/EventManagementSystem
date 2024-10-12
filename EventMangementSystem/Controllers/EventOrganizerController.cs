using EventMangementSystem.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace EventMangementSystem.Controllers
{
    public class EventOrganizerController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();


        public ActionResult ServiceRequests()
        {
            // Fetch all the service requests from the database
            var serviceRequests = db.ServiceRequests.Where(x=>x.Event.EventMangerEmail == User.Identity.Name).ToList();

            // Pass the list of service requests to the view
            return View(serviceRequests);
        }
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

        // GET: View bids for a specific service request
        [HttpGet]
        public ActionResult ViewBids(int requestId)
        {
            // Disable lazy loading and load bids explicitly
            db.Configuration.LazyLoadingEnabled = false;

            var request = db.ServiceRequests.Include("Bids").FirstOrDefault(r => r.Id == requestId);
            if (request == null)
            {
                return HttpNotFound();
            }

            return View(request);
        }

        // POST: Select a bid for a service request
        [HttpPost]
      
        public ActionResult SelectBid(int requestId, int bidId)
        {
            var request = db.ServiceRequests.Find(requestId);
            if (request == null)
            {
                return HttpNotFound();
            }

            var selectedBid = request.Bids.FirstOrDefault(b => b.Id == bidId);
            if (selectedBid == null)
            {
                return HttpNotFound();
            }

            // Assign the selected service provider
            request.IsAssigned = true;
            request.ServiceProviderId = selectedBid.ServiceProviderId;
            db.SaveChanges();

            return RedirectToAction("ConfirmedRequests");
        }


        [HttpGet]
        public ActionResult ConfirmedRequests()
        {
            // Fetch all confirmed service requests
            var confirmedRequests = db.ServiceRequests
                                      .Where(r => r.IsAssigned)
                                      .ToList();

            return View(confirmedRequests);
        }
        // GET: Review contract for the selected service provider
        [HttpGet]
        public ActionResult ReviewContract(int requestId)
        {
            // Disable lazy loading and load the necessary data
            db.Configuration.LazyLoadingEnabled = false;

            var request = db.ServiceRequests.Include("Bids").FirstOrDefault(r => r.Id == requestId);
            if (request == null)
            {
                return HttpNotFound();
            }

            var selectedBid = request.Bids.FirstOrDefault(b => b.ServiceProviderId == request.ServiceProviderId);
            if (selectedBid == null)
            {
                return HttpNotFound();
            }

            return View(selectedBid);
        }

        // POST: Confirm contract after reviewing it
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmContract(int requestId)
        {
            // Here you can integrate payment logic or redirect to a payment provider
            // You can also confirm contract details and redirect accordingly

            return RedirectToAction("MakePayment", new { requestId });
        }
    }
}
