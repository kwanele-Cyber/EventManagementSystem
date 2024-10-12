using EventMangementSystem.Hubs;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using EventMangementSystem.Models;
using System.Diagnostics;

namespace EventMangementSystem.Controllers
{
    [System.Web.Mvc.Authorize]
    public class ChatController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public JsonResult GetEvents()
        {
            if (User.IsInRole("EventManager"))
            {
                var events = db.Events
                    .Where(e => e.EventMangerEmail == User.Identity.Name)
                    .Select(e => new
                    {
                        e.EventId,
                        e.Name
                    }).ToList();

                return Json(events, JsonRequestBehavior.AllowGet);
            }
            else if (User.IsInRole("Admin"))
            {
                var events = db.Events.Select(e => new
                {
                    e.EventId,
                    e.Name
                }).ToList();

                return Json(events, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var eventIds = db.Tickets
                    .Where(t => t.AttendeeEmail == User.Identity.Name)
                    .Select(t => t.EventId)
                    .Distinct()
                    .ToList();

                var events = db.Events
                    .Where(e => eventIds.Contains(e.EventId))
                    .Select(e => new
                    {
                        e.EventId,
                        e.Name
                    }).ToList();

                return Json(events, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Index(int id)
        {
            try
            {
                var chatSession = db.ChatSessions.Include(c => c.ChatMessages.Select(m => m.User))
                                                  .FirstOrDefault(c => c.EventId == id);

                if (chatSession == null)
                {
                    chatSession = new ChatSession { EventId = id };
                    db.ChatSessions.Add(chatSession);
                    db.SaveChanges();
                }

                if (Request.IsAjaxRequest())
                {
                    var result = new
                    {
                        ChatSessionId = chatSession.ChatSessionId,
                        UserId = User.Identity.GetUserId()
                    };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                ViewBag.ChatSessionId = chatSession.ChatSessionId;
                ViewBag.UserId = User.Identity.GetUserId();
                ViewBag.EventId = id;

                return View(chatSession);
            }
            catch (Exception ex)
            {
                // Log the error details
                Debug.WriteLine($"Error in ChatController.Index: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);

                // Return a 500 status code with error message
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpPost]
        public ActionResult SendMessage(int chatSessionId, string userId, string message)
        {
            try
            {
                if (string.IsNullOrEmpty(message))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Message cannot be empty");
                }

                var chatMessage = new ChatMessage
                {
                    ChatSessionId = chatSessionId,
                    UserId = userId,
                    Message = message,
                    Timestamp = DateTime.Now
                };

                db.ChatMessages.Add(chatMessage);
                db.SaveChanges();

                var context = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                context.Clients.Group(chatSessionId.ToString()).broadcastMessage(userId, message, DateTime.Now.ToString("g"));

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "An error occurred while sending the message");
            }
        }

        public JsonResult GetMessagesForEvent(int eventId)
        {
            var messages = db.ChatMessages
                .Where(m => m.ChatSession.EventId == eventId)
                .OrderBy(m => m.Timestamp)
                .ToList() // Fetch the data first
                .Select(m => new
                {
                    m.Message,
                    Timestamp = m.Timestamp.ToString("o"), // Convert to ISO 8601 format
                    UserName = m.User.UserName
                })
                .ToList();

            return Json(messages, JsonRequestBehavior.AllowGet);
        }


    }
}
