using EventMangementSystem.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EventMangementSystem.Controllers
{
    public class ServiceProviderController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [HttpGet]
        [Authorize(Roles = "ServiceProvider")]
        public ActionResult OpenBids()
        {
            var provider = db.ServiceProviders.Where(x => x.email == User.Identity.Name).FirstOrDefault();
            var providerBids = db.Quotations
                                 .Where(bid => bid.ServiceProviderId == provider.Id)
                                 .Include(bid => bid.ServiceRequest)
                                 .ToList();
            return View(providerBids);
        }
        public ActionResult ViewServiceRequests()
        {
            var serviceRequests = db.ServiceRequests
                                    .Where(r => !r.IsAssigned)
                                    .ToList();
            return View(serviceRequests);
        }
        [HttpPost]

      
        public ActionResult CompleteService(int requestId, string eventManagerSignatureData, string serviceProviderSignatureData)
        {
            var request = db.ServiceRequests.Find(requestId);

            if (request == null || request.IsCompleted)
            {
                return HttpNotFound();
            }

            // Mark the request as completed
            request.IsCompleted = true;
            request.EventManagerSignature = eventManagerSignatureData;
            request.ServiceProviderSignature = serviceProviderSignatureData;
            db.SaveChanges();

            return RedirectToAction("ConfirmedBids", "ServiceProvider");
        }

        [HttpGet]

        public ActionResult ConfirmedBids()
        {
            // Get the logged-in service provider's ID
            var provider = db.ServiceProviders.Where(x => x.email == User.Identity.Name).FirstOrDefault();

            // Fetch all bids that were confirmed/accepted by event organizers
            var confirmedBids = db.Quotations
                                  .Where(q => q.ServiceProviderId == provider.Id && q.ServiceRequest.IsAssigned)
                                  .Include(q => q.ServiceRequest) // Include related service request data
                                  .Include(q => q.ServiceRequest.Event) // Include related event data
                                  .ToList();

            return View(confirmedBids);
        }
        [HttpGet]

        public ActionResult SubmitBid(int requestId)
        {
            var request = db.ServiceRequests.Find(requestId);
            var provider = db.ServiceProviders.Where(x => x.email == User.Identity.Name).FirstOrDefault();
            if (request == null)
            {
                return HttpNotFound();
            }
            var bid = new Quotation
            {
                ServiceRequestId = requestId,
                ServiceProviderId = provider.Id
            };
            ViewBag.Request = request; // To show the service request details if needed in the view
            return View(bid); // Pass the Quotation model
        }

        [HttpPost]

        public ActionResult SubmitBid(Quotation bid)
        {
            var request = db.ServiceRequests.Find(bid.ServiceRequestId);
            request.Bids.Add(bid); // Add the bid to the service request
            db.SaveChanges();
            return RedirectToAction("OpenBids");
        }
        // Controllers/ServiceProviderController.cs

        [HttpGet]

        public ActionResult CheckInventory(int serviceProviderId)
        {
            var inventory = db.Inventories2.Where(i => i.ServiceProviderId == serviceProviderId && i.IsAvailable).ToList();
            return View(inventory);
        }

        [HttpPost]

        public ActionResult ConfirmInventory(int inventoryId, int requestId)
        {
            var inventory = db.Inventories2.Find(inventoryId);
            if (inventory.IsAvailable)
            {
                // Mark equipment as reserved
                inventory.IsAvailable = false;
                db.SaveChanges();
                return RedirectToAction("ConfirmService");
            }
            else
            {
                // Notify that equipment is unavailable
                return RedirectToAction("InventoryError");
            }
        }
        [HttpGet]
        [Authorize(Roles = "ServiceProvider, EventManager")]
        public ActionResult ConfirmService(int bidId)
        {
            var bid = db.Quotations.Include("ServiceRequest.Event").FirstOrDefault(b => b.Id == bidId);

            if (bid == null)
            {
                return HttpNotFound();
            }

            return View(bid);
        }

        // POST: ConfirmService
        [HttpPost]
        [Authorize(Roles = "ServiceProvider, EventManager")]
  
        public ActionResult ConfirmService(int bidId, string eventManagerSignatureData, string serviceProviderSignatureData)
        {
            var bid = db.Quotations.Include("ServiceRequest").FirstOrDefault(b => b.Id == bidId);

            if (bid == null)
            {
                return HttpNotFound();
            }

            // Mark the service as completed in the service request
            var serviceRequest = bid.ServiceRequest;
            serviceRequest.IsCompleted = true;
            serviceRequest.EventManagerSignature = eventManagerSignatureData;
            serviceRequest.ServiceProviderSignature = serviceProviderSignatureData;

            db.SaveChanges();
            TempData["SuccessMessage"] = "Service Delivery confirmed";
            return RedirectToAction("ConfirmedBids", "ServiceProvider");
        }
    }


}
