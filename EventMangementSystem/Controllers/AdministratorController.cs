using EventMangementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EventMangementSystem.Controllers
{
    public class AdministratorController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // Controllers/AdministratorController.cs

        [HttpGet]
        public ActionResult OpenForBidding(int requestId)
        {
            var request = db.ServiceRequests.Find(requestId);
            request.IsOpenForBidding = true;
            db.SaveChanges();
            return RedirectToAction("PendingRequests");
        }
        // GET: Display the form to create a new Service Category
        [HttpGet]
      
        public ActionResult CreateServiceCategory()
        {
            return View();
        }

        // POST: Handle the form submission and save the new Service Category
        [HttpPost]
       
        [ValidateAntiForgeryToken]
        public ActionResult CreateServiceCategory(ServiceCategory category)
        {
            if (ModelState.IsValid)
            {
                db.ServiceCategories.Add(category);
                db.SaveChanges();
                return RedirectToAction("ServiceCategoriesList");  // Redirect to the list of categories after creation
            }

            return View(category);
        }

        // You can also add an action to list all categories if needed
        [HttpGet]
      
        public ActionResult ServiceCategoriesList()
        {
            var categories = db.ServiceCategories.ToList();
            return View(categories);
        }

    }
}