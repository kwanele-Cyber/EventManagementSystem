using EventMangementSystem.Models;
using EventMangementSystem.ViewModels;
using Microsoft.AspNet.Identity;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.EnterpriseServices;

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


        [HttpGet]
        [Authorize(Roles = "ServiceProvider")]
        public ActionResult StartService(int bidId)
        {
            var bid = db.Quotations.Include("ServiceRequest").FirstOrDefault(b => b.Id == bidId);

            if (bid == null || bid.ServiceRequest == null)
            {
                return HttpNotFound();
            }

            var serviceRequest = bid.ServiceRequest;

            // Generate a random 4-digit code for starting the service
            var random = new Random();
            serviceRequest.StartCode = random.Next(1000, 9999).ToString();

            // Generate a QR code for completing the service
            serviceRequest.FinishCode = GenerateQRCode(serviceRequest.Id);

            // Update the status of the service request
            serviceRequest.Status = ServiceRequestStatus.Assigned;

            db.SaveChanges();

            // Send an email to the Event Organizer/Event Manager
            var eventManagerEmail = serviceRequest.Event.EventMangerEmail; // Ensure the event has a manager's email
            SendEmailWithQRCode(eventManagerEmail, serviceRequest.StartCode, serviceRequest.FinishCode);

            // Redirect the service provider to the verification page
            return RedirectToAction("VerifyQRCode", "ServiceProvider", new { bidId = bidId });
        }


        [HttpGet]
        public ActionResult VerifyQRCode(int bidId)
        {
            var bid = db.Quotations.Include("ServiceRequest").FirstOrDefault(b => b.Id == bidId);

            if (bid == null || bid.ServiceRequest == null)
            {
                return HttpNotFound();
            }

            var serviceRequest = bid.ServiceRequest;

            // Differentiate between roles (EventManager and ServiceProvider)
            if (User.IsInRole("EventManager"))
            {
                // EventManager should see the QR code and the start pin
                return View("VerifyQRCodeEventManager", serviceRequest);
            }
            else if (User.IsInRole("ServiceProvider"))
            {
                // ServiceProvider should scan or enter the QR code
                return View("VerifyQRCodeServiceProvider", serviceRequest);
            }

            return new HttpUnauthorizedResult(); // Unauthorized access if neither role is matched
        }


        //For verifying StartService
        [HttpPost]
        public ActionResult VerifyQRCode(int bidId, string enteredCode)
        {
            var bid = db.Quotations.Include("ServiceRequest").FirstOrDefault(b => b.ServiceRequest.Id == bidId);

            if (bid == null || bid.ServiceRequest == null)
            {
                return HttpNotFound();
            }

            var serviceRequest = bid.ServiceRequest;

            // Check if the entered code matches the StartCode
            if (serviceRequest.StartCode == enteredCode)
            {
                // Change status to InProgress and confirm start
                serviceRequest.Status = ServiceRequestStatus.InProgress;
                serviceRequest.IsAssigned = true;
                serviceRequest.IsCompleted = false;

                db.SaveChanges();

                TempData["SuccessMessage"] = "Service started successfully.";
                return RedirectToAction("ConfirmedBids", "ServiceProvider");
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid code. Please try again.";
                return RedirectToAction("VerifyQRCode", new { bidId = bidId });
            }
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
            // Mark the service as completed
            request.Status = ServiceRequestStatus.Completed;
            request.IsCompleted = true;

            request.EventManagerSignature = eventManagerSignatureData;
            request.ServiceProviderSignature = serviceProviderSignatureData;
            db.SaveChanges();

            return RedirectToAction("ConfirmedBids", "ServiceProvider");
        }

        [HttpGet]
        public ActionResult ConfirmedBids()
        {
            var provider = db.ServiceProviders.FirstOrDefault(x => x.email == User.Identity.Name);

            // Check if the provider is null
            if (provider == null)
            {
                // If the provider is null, you can either return an error or handle it gracefully
                TempData["ErrorMessage"] = "Service provider not found. Please ensure you are logged in with the correct account.";
                return RedirectToAction("Index", "Home"); // Redirect to a suitable page, e.g., home or login page
            }

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
            // Fetch the bid and associated service request
            var bid = db.Quotations.Include("ServiceRequest").FirstOrDefault(b => b.Id == bidId);

            // Return a 404 error if the bid is not found
            if (bid == null)
            {
                return HttpNotFound();
            }

            // Get the service request from the bid
            var serviceRequest = bid.ServiceRequest;

            // Set the service request status and mark it as completed
            serviceRequest.Status = ServiceRequestStatus.Completed;
            serviceRequest.IsCompleted = true;

            // Save the signatures for both the event manager and service provider
            serviceRequest.EventManagerSignature = eventManagerSignatureData;
            serviceRequest.ServiceProviderSignature = serviceProviderSignatureData;

            db.SaveChanges();

            // Show a success message & Redirect back to the ConfirmedBids page
            TempData["SuccessMessage"] = "Service Delivery confirmed";

            return RedirectToAction("ConfirmedBids", "ServiceProvider");
        }

        // GET: ServiceProvider/PerformService
        [HttpGet]
        public ActionResult PerformService(int bidId, string returnUrl = null)
        {
            var bid = db.Quotations
                .Include(q => q.ServiceRequest)
                .Include(q => q.ServiceRequest.Team)
                .Include(q => q.ServiceRequest.Team.GroupTasks)
                .FirstOrDefault(q => q.Id == bidId);

            if (bid == null)
            {
                return HttpNotFound();
            }

            var serviceRequest = bid.ServiceRequest;

            // Fetch all tasks related to this service request
            var tasks = bid.ServiceRequest.Team.GroupTasks.ToList();

            var viewModel = new PerformServiceViewModel
            {
                ServiceRequest = serviceRequest,
                Tasks = tasks,
                ReturnUrl = returnUrl,
                BidId = bid.Id,
            };

            return View(viewModel);
        }

        // POST: ServiceProvider/CompleteTask
        [HttpPost]
        public ActionResult CompleteTask(int taskId, int? serviceRequestId = null, string returnUrl = null)
        {
            //TODO:To be fixed
            var task = db.Tasks.Find(taskId);

            var bid = db.Quotations
                .Include(t => t.ServiceProvider)
                .Include(t => t.ServiceRequest)
                .Include(t => t.ServiceRequest.GroupTasks)
                .Where(t => t.ServiceRequest.TeamId == task.TeamId).FirstOrDefault();

            if (task == null)
            {
                return HttpNotFound();
            }

            task.Status = GroupTaskStatus.Completed;
            task.Progress = 100;
            task.ActualEndTime = DateTime.Now;

            db.Entry(task).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("PerformService", new { bidId = bid.Id , returnUrl = returnUrl});
        }

        // POST: ServiceProvider/InspectServiceDelivery
        [HttpPost]
        public ActionResult InspectServiceDelivery(int bidId, int serviceRequestId, string returnUrl = null)
        {
            var serviceRequest = db.ServiceRequests.Find(serviceRequestId);

            if (serviceRequest == null)
            {
                return HttpNotFound();
            }

            serviceRequest.Status = ServiceRequestStatus.UnderInspection;
            db.Entry(serviceRequest).State = EntityState.Modified;
            db.SaveChanges();

            TempData["SuccessMessage"] = "Service delivery under Inspection";
            return RedirectToAction("ConfirmService", new {bidId = bidId});
        }



        //Begining of dashboad

        public async Task<ActionResult> Dashboard()
        {
            var provider = db.ServiceProviders
                .Where(x => x.email == User.Identity.Name).FirstOrDefault();
            var serviceProviderId = provider.Id;

            var quotations = await db.Quotations
                .Include(q => q.ServiceRequest)
                .Include(q => q.ServiceRequest.Team)
                .Where(q => q.ServiceProviderId == serviceProviderId)
                .ToListAsync();



            var upcomingBookings = quotations
                .Where(q => q.ServiceRequest.IsCompleted == false )
                .Select(q => new UpcomingBooking
                {
                    EventName = q.ServiceRequest.Event.Name,
                    EventDateTime = q.ServiceRequest.Event.Start,
                    Status = q.ServiceRequest.Status.ToString(),
                    Id = q.ServiceRequest.Id,
                    ServiceProviderId = serviceProviderId,
                })
                .ToList();


            var totalEarnings = quotations
                .Where(q => q.ServiceRequest.IsCompleted)
                .Sum(q => q.Price);
            var upcomingBookingsCount = upcomingBookings.Count;

            var revenueData = await db.Quotations
            .Where(q => q.ServiceProviderId == serviceProviderId && q.ServiceRequest.IsCompleted == true)
            .Select(q => new
            {
                TransactionId = q.Id, 
                Revenue = q.Price
                }       )
            .ToListAsync();

            // Create labels and amounts based on each transaction
            var revenueLabels = revenueData.Select(d => $"Transaction {d.TransactionId}").ToList(); // Use a descriptive label
            var revenueAmounts = revenueData.Select(d => d.Revenue).ToList();

            var viewModel = new ServiceProviderDashboardViewModel
            {
                TotalEarnings = totalEarnings,
                UpcomingBookingsCount = upcomingBookingsCount,
                UpcomingBookings = upcomingBookings,
                RevenueLabels = revenueLabels,
                RevenueData = revenueAmounts
            };

            return View(viewModel);
        }
        //End of dashboad

        #region HelperMethods
        private string GenerateQRCode(int requestId)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(requestId.ToString(), QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);

            using (MemoryStream ms = new MemoryStream())
            {
                Bitmap qrCodeImage = qrCode.GetGraphic(20); // Adjust the size as needed
                qrCodeImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                byte[] byteImage = ms.ToArray();

                // Convert the byte array to Base64 for storage
                return Convert.ToBase64String(byteImage);
            }
        }

        private void SendEmailWithQRCode(string eventManagerEmail, string startCode, string finishQRCode)
        {
            try
            {
                var email = new MailMessage
                {
                    From = new MailAddress("eventproplanners@gmail.com"),
                    Subject = "Service Start Information",
                    Body = $@"
                Service has started successfully. Below are the details:
                
                Start Code: {startCode}
                
                Scan the attached QR code to complete the service.
                
                Regards,
                EventManagement Team"
                };

                // Add the recipient (Event Manager's email)
                email.To.Add(eventManagerEmail);

                // Convert the Base64 QR code string into a byte array
                byte[] qrCodeBytes = Convert.FromBase64String(finishQRCode);

                // Save the QR code image to the server temporarily
                string qrCodeFileName = $"{Guid.NewGuid()}.png";
                string qrCodeFilePath = Path.Combine(Server.MapPath("~/images"), qrCodeFileName);
                System.IO.File.WriteAllBytes(qrCodeFilePath, qrCodeBytes);

                // Attach the QR code image to the email
                Attachment qrCodeAttachment = new Attachment(qrCodeFilePath, MediaTypeNames.Image.Jpeg);
                email.Attachments.Add(qrCodeAttachment);

                // Send the email
                var smtpClient = new SmtpClient();
                smtpClient.Send(email);

                // Cleanup: Optionally delete the QR code file from the server after sending
                if (System.IO.File.Exists(qrCodeFilePath))
                {
                    System.IO.File.Delete(qrCodeFilePath);
                }

                TempData["SuccessMessage"] = "Email sent successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to send email due to: " + ex.Message;
            }
        }
        #endregion

    }


}
