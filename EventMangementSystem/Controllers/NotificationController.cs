using System;
using System.Linq;
using System.Web.Mvc;
using EventMangementSystem.Models;
using Microsoft.AspNet.Identity;

namespace EventMangementSystem.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        // GET: Notification
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var notifications = _context.Notifications
                                        .Where(n => n.UserId == userId)
                                        .OrderByDescending(n => n.CreatedAt)
                                        .ToList();

            return View(notifications);
        }

        // POST: Notification/MarkAsRead/5
        [HttpPost]
        public ActionResult MarkAsRead(int id)
        {
            var userId = User.Identity.GetUserId();
            var notification = _context.Notifications.SingleOrDefault(n => n.NotificationId == id && n.UserId == userId);

            if (notification != null)
            {
                notification.IsRead = true;
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        // POST: Notification/Delete/5
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var userId = User.Identity.GetUserId();
            var notification = _context.Notifications.SingleOrDefault(n => n.NotificationId == id && n.UserId == userId);

            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        // GET: Notification/GetUnreadNotifications
        public ActionResult GetUnreadNotifications()
        {
            var userId = User.Identity.GetUserId();
            var unreadNotifications = _context.Notifications
                                              .Where(n => n.UserId == userId && !n.IsRead)
                                              .OrderByDescending(n => n.CreatedAt)
                                              .ToList();

            return PartialView("_NotificationPartial", unreadNotifications);
        }
    }
}
