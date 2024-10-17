
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using EventMangementSystem.Models;

namespace EventMangementSystem.Controllers
{
    public class ServiceRequestsController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // GET: ServiceRequests/Index
        public ActionResult Index(int? id, int? teamId, int? eventId, int? serviceProviderId)
        {
            var serviceRequests = db.ServiceRequests.Include(s => s.Event)
                                                    .Include(s => s.ServiceProvider)
                                                    .Include(s => s.Team)
                                                    .AsQueryable();

            if (id != null)
            {
                serviceRequests = serviceRequests.Where(s => s.Id == id);
            }

            if (teamId != null)
            {
                serviceRequests = serviceRequests.Where(s => s.TeamId == teamId);
            }

            if (eventId != null)
            {
                serviceRequests = serviceRequests.Where(s => s.EventId == eventId);
            }

            if (serviceProviderId != null)
            {
                serviceRequests = serviceRequests.Where(s => s.ServiceProviderId == serviceProviderId);
            }

            return View(serviceRequests.ToList());
        }

        // GET: ServiceRequests/Details/{id}
        public ActionResult Details(int id)
        {
            var serviceRequest = db.ServiceRequests
                                   .Include(s => s.Event)
                                   .Include(s => s.ServiceProvider)
                                   .Include(s => s.Team)
                                   .FirstOrDefault(s => s.Id == id);

            if (serviceRequest == null)
            {
                return HttpNotFound();
            }

            return View(serviceRequest);
        }

        // Additional actions like Create, Edit, Delete, etc.

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
