using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using EventMangementSystem.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using QRCoder;
using Image = iTextSharp.text.Image;

namespace EventMangementSystem.Controllers
{
    public class DriverAssignmentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var Inventories = db.EventInventories.Include(x => x.Inventory).ToList();
            return View(Inventories);
        }

        public ActionResult MyAssignments(string email = "Email")
        {
            if (email == "Email")
            {
                email = User.Identity.Name;
                ViewBag.Title = "MyAssignments";
            }
            else
            {
                ViewBag.Title = "Driver Assignments";
            }
            var assignments = db.DriverAssignments.Where(x => x.Email == email).Include(x => x.EventInventory).ToList();
            return View(assignments);
        }

        public ActionResult InventoryReady(int id)
        {
            var assignment = db.DriverAssignments.Find(id);
            var inventory = db.EventInventories.Find(assignment.EventInventoryId);
            inventory.Status = "Ready for pickup";
            assignment.Status = inventory.Status;
            db.Entry(assignment).State = EntityState.Modified;
            db.Entry(inventory).State = EntityState.Modified;
            db.SaveChanges();
            SendInventoryReadyEmail(assignment);
            TempData["SuccessMessage"] = "Inventory marked as ready for delivery, Email sent to driver.";
            return RedirectToAction("Index");
        }

        public ActionResult DispatchInventory(int id)
        {
            var inventory = db.EventInventories
                              .Include(e => e.Inventory) // Ensure Inventory is included
                              .FirstOrDefault(e => e.EventInventoryId == id);

            if (inventory == null)
            {
                TempData["ErrorMessage"] = "Inventory Not Found.";
                return RedirectToAction("Index");
            }

            var viewModel = new DispatchInventoryViewModel
            {
                EventInventory = inventory,
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignAndGeneratePdf(int id, string driverSignature, string adminSignature)
        {
            try
            {
                var inventory = db.EventInventories.Find(id);
                var assign = db.DriverAssignments.Where(x => x.EventInventoryId == id).FirstOrDefault();
                if (inventory == null)
                {
                    TempData["ErrorMessage"] = "Inventory not found. Please try again.";
                    return RedirectToAction("Index");
                }

                // Convert and validate the driver signature
                try
                {
                    inventory.DriverSignature = ConvertToBase64(driverSignature);
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Invalid driver signature format: " + ex.Message;
                    return RedirectToAction("Index");
                }

                // Convert and validate the admin signature
                try
                {
                    inventory.AdminSignature = ConvertToBase64(adminSignature);
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Invalid admin signature format: " + ex.Message;
                    return RedirectToAction("Index");
                }

                assign.Status = "Dispatched";
                inventory.Status = "Dispatched";
                db.Entry(assign).State = EntityState.Modified;
                db.Entry(inventory).State = EntityState.Modified;
                db.SaveChanges();

                string pdfPath = Path.Combine(Server.MapPath("~/assets"), $"inventory_{id}.pdf");
                GeneratePdf(inventory, pdfPath);

                TempData["SuccessMessage"] = "Inventory dispatched successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        private string ConvertToBase64(string rawSignatureData)
        {
            if (IsBase64String(rawSignatureData))
            {
                return rawSignatureData;
            }

            try
            {
                if (rawSignatureData.StartsWith("data:image/png;base64,"))
                {
                    rawSignatureData = rawSignatureData.Replace("data:image/png;base64,", string.Empty);
                }

                byte[] signatureBytes = Convert.FromBase64String(rawSignatureData);
                return Convert.ToBase64String(signatureBytes);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to convert signature to Base64 format: " + ex.Message);
            }
        }

        private bool IsBase64String(string base64String)
        {
            if (string.IsNullOrEmpty(base64String) || base64String.Length % 4 != 0 ||
                base64String.Contains(" ") || base64String.Contains("\t") || base64String.Contains("\r") || base64String.Contains("\n"))
                return false;

            try
            {
                Convert.FromBase64String(base64String);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private void GeneratePdf(EventInventory inventory, string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                Document document = new Document(PageSize.A4, 50, 50, 25, 25); // Set margins
                PdfWriter writer = PdfWriter.GetInstance(document, fs);
                document.Open();

                // Define fonts
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var textFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);

                // Add title
                document.Add(new Paragraph("Inventory Details", titleFont) { Alignment = Element.ALIGN_CENTER });

                // Add inventory details
                document.Add(new Paragraph($"Inventory ID: {inventory.EventInventoryId}", textFont));
                document.Add(new Paragraph($"Delivery Date: {(inventory.DeliveryDate != DateTime.MinValue ? inventory.DeliveryDate.ToString("MM/dd/yyyy") : "N/A")}", textFont));
                document.Add(new Paragraph($"Status: {inventory.Status}", textFont));

                // Add Driver's Signature
                document.Add(new Paragraph("Driver's Signature:", textFont));
                if (!string.IsNullOrEmpty(inventory.DriverSignature))
                {
                    Image driverSignatureImage = Image.GetInstance(Convert.FromBase64String(inventory.DriverSignature));
                    driverSignatureImage.ScaleToFit(200f, 100f);
                    document.Add(driverSignatureImage);
                }
                else
                {
                    document.Add(new Paragraph("Not signed", textFont));
                }

                // Add some space
                document.Add(new Paragraph(" "));

                // Add Admin's Signature
                document.Add(new Paragraph("Admin's Signature:", textFont));
                if (!string.IsNullOrEmpty(inventory.AdminSignature))
                {
                    Image adminSignatureImage = Image.GetInstance(Convert.FromBase64String(inventory.AdminSignature));
                    adminSignatureImage.ScaleToFit(200f, 100f);
                    document.Add(adminSignatureImage);
                }
                else
                {
                    document.Add(new Paragraph("Not signed", textFont));
                }

                // Add a line break
                document.Add(new Paragraph(" "));

                // Add inventory items
                document.Add(new Paragraph("Inventories:", textFont));
                foreach (var item in inventory.Inventories)
                {
                    document.Add(new Paragraph($"- {item.ItemName}: {item.QuantityAvailable}", textFont));
                }

                document.Close();
                writer.Close();
            }
        }

        public ActionResult Dispatchedinventorys()
        {
            var dispatchedInventories = db.EventInventories
                                     .Where(o => o.Status == "Dispatched")
                                     .ToList();

            return View(dispatchedInventories);
        }

        public ActionResult DownloadPdf(int id)
        {
            string pdfPath = Path.Combine(Server.MapPath("~/assets"), $"inventory_{id}.pdf");
            if (!System.IO.File.Exists(pdfPath))
            {
                TempData["ErrorMessage"] = "PDF not found.";
                return RedirectToAction("DispatchedInventoryDetails", new {id = id});
            }

            byte[] pdfContent = System.IO.File.ReadAllBytes(pdfPath);
            return File(pdfContent, "application/pdf", $"Inventory_{id}.pdf");
        }


        public ActionResult DispatchedInventoryDetails(int id)
        {
            var inventory = db.EventInventories
                          .Include(o => o.Inventories)
                          .FirstOrDefault(o => o.EventInventoryId == id);

            if (inventory == null)
            {
                TempData["ErrorMessage"] = "Inventory not found.";
                return RedirectToAction("DispatchedInventories");
            }

            return View(inventory);
        }

        private void SendInventoryReadyEmail(DriverAssignment assignment)
        {
            var email = new MailMessage
            {
                From = new MailAddress("DbnEventMangement@outlook.com"),
                Subject = "Inventory Ready | " + assignment.EventInventoryId,
                Body = $"Inventory Number: {assignment.EventInventoryId}\t\tDelivery Date: {assignment.DeliveryDate}\t\t Delivery Time: {assignment.DeliveryTime}\n\n" +
                       $"Hi {assignment.Driver.Name},\n\n" +
                       $"Please note that Inventory {assignment.EventInventoryId} is ready to be picked up for delivery to address. Please proceed with inventory delivery within due time.\n\n" +
                       "Regards,\r\nEventManagement Team"
            };
            email.To.Add(assignment.Email);

            var smtpClient = new SmtpClient();
            smtpClient.Send(email);
        }

        public ActionResult StartInventoryDelivery(int id, string time)
        {
            var assignment = db.DriverAssignments.Find(id);
            assignment.DeliveryTime = time;
            var inventory = db.EventInventories.Find(assignment.EventInventoryId);
            assignment.Status = "On the way";
            inventory.Status = "On the way";
            db.Entry(assignment).State = EntityState.Modified;
            db.Entry(inventory).State = EntityState.Modified;

            try
            {
                var email2 = new MailMessage
                {
                    From = new MailAddress("DbnEventMangement@outlook.com"),
                    Subject = "Inventory Delivery Started | " + assignment.EventInventoryId,
                    Body = $"Inventory Number: " + assignment.EventInventoryId + "\t\t Estimated Arrival Time: " + time + " \n\n" +
                           $"Hi {assignment.Driver.Name}, \n\n" +
                           $"Please note that, inventory {assignment.EventInventoryId} is picked up by driver for delivery to address .\n\n" +
                           $"Your inventory is now out for delivery. The driver will be at your venue around {time}.\n\n" +
                           $"Please present this unique code to the driver: {inventory.UniqueCode}\n\n or the attached QR Code.\n\n" +
                           "Regards,\r\nEventManagement Team"
                };
                var _event = db.Events.Find(inventory.EventId);
                email2.To.Add(_event.EventMangerEmail);

                string imagePath = Server.MapPath("~/images/" + inventory.QrCodePicture);
                Attachment attachment = new Attachment(imagePath, MediaTypeNames.Image.Jpeg);
                email2.Attachments.Add(attachment);

                var smtpClient = new SmtpClient();
                smtpClient.Send(email2);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to send email due to, " + ex.Message;
                return RedirectToAction("MyAssignments");
            }

            db.SaveChanges();
            TempData["SuccessMessage"] = "Inventory Delivery Successfully Started. Please Navigate to Event Location";
            return RedirectToAction("Navigation", new { id = inventory.EventInventoryId });
        }

        public ActionResult Navigation(int id)
        {
            HttpCookie cookie = new HttpCookie("OrdID")
            {
                Value = id.ToString(),
                Expires = DateTime.Now.AddDays(1)
            };
            Response.Cookies.Add(cookie);

            var inventory = db.EventInventories.Find(id);
            ViewBag.DestinationAddress = inventory.Address;
            return View();
        }

        public ActionResult FinishInventoryDelivery()
        {
            return View();
        }

        [HttpPost]
        public ActionResult FinishInventoryDelivery(string code)
        {
            HttpCookie cookie = Request.Cookies["OrdID"];
            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            {
                string ordId = cookie.Value;
                int id = int.Parse(ordId);
                var inventory = db.EventInventories.Find(id);

               
                if (int.Parse(code) == inventory.UniqueCode)
                {
                    return RedirectToAction("ConfirmDelivery", new { id = id });
                }
                else
                {
                    TempData["ErrorMessage"] = "Incorrect Code, Please Try Again";
                    return RedirectToAction("FinishInventoryDelivery");
                }
                
            }
            else
            {
                TempData["ErrorMessage"] = "Something went wrong";
                return RedirectToAction("MyAssignments");
            }
        }
        public ActionResult ConfirmDelivery(int id)
        {
            // Retrieve the EventInventory based on the provided ID
            var eventInventory = db.EventInventories
                                   .Include(e => e.Inventory) // Ensure the Inventory is included
                                   .FirstOrDefault(e => e.EventInventoryId == id);

            if (eventInventory == null)
            {
                TempData["ErrorMessage"] = "Inventory not found.";
                return RedirectToAction("Index");
            }

            // Create a view model to pass data to the view
            var viewModel = new DispatchInventoryViewModel
            {
                EventInventory = eventInventory
            };

            // Render the ConfirmDelivery view with the view model
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmDelivery(int id, string driverSignature, string managerSignature)
        {
            try
            {
                var assign = db.DriverAssignments.Where(x => x.EventInventoryId == id && x.Email == User.Identity.Name).FirstOrDefault();

                var inventory = db.EventInventories.Find(id);
                if (inventory == null)
                {
                    TempData["ErrorMessage"] = "Inventory not found.";
                    return RedirectToAction("Index");
                }

                // Validate driver signature
                try
                {
                    inventory.DriverSignature = ConvertToBase64(driverSignature);
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Invalid driver signature format: " + ex.Message;
                    return RedirectToAction("Index");
                }

                // Validate event manager's signature
                try
                {
                    inventory.ManagerSignature = ConvertToBase64(managerSignature);
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Invalid event manager signature format: " + ex.Message;
                    return RedirectToAction("Index");
                }


                inventory.Status = "Inventory Received";
                inventory.DeliveredOn = DateTime.Now;
                inventory.DeliveredBy = assign.DrivId;
                assign.Status = "Completed";
                assign.IsActive = false;
                try
                {
                    var email2 = new MailMessage
                    {
                        From = new MailAddress("DbnEventMangement@outlook.com"),
                        Subject = "Inventory Delivered",
                        Body = $"Inventory Number: " + inventory.EventInventoryId + " \n\n" +
                               $"Hi {inventory.FirstName}, \n\n" +
                               $"Your inventory {inventory.EventInventoryId} has been delivered to {inventory.Address} on {DateTime.Now.Date.ToLongDateString()} at {DateTime.Now.ToShortTimeString()}.\n\n" +
                               "Regards,\r\nEventManagement Team"
                    };
                    var _event = db.Events.Find(inventory.EventId);
                    email2.To.Add(_event.EventMangerEmail);

                    var smtpClient = new SmtpClient();
                    smtpClient.Send(email2);
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Failed to send email due to, " + ex.Message;
                    return RedirectToAction("FinishInventoryDelivery");
                }

                db.SaveChanges();
                inventory.Status = "Delivered";
                db.Entry(inventory).State = EntityState.Modified;
                db.SaveChanges();

                TempData["SuccessMessage"] = "Delivery confirmed successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred: " + ex.Message;
                return RedirectToAction("Index");
            }
        }



        [HttpPost]
        public ActionResult NoResponseAction()
        {
            HttpCookie cookie = Request.Cookies["InvID"];
            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            {
                string ordId = cookie.Value;
                int id = int.Parse(ordId);
                var inventory = db.EventInventories.Find(id);

                var assign = db.DriverAssignments.Where(x => x.EventInventoryId == id).FirstOrDefault();
                db.Entry(inventory).State = EntityState.Modified;
                db.Entry(assign).State = EntityState.Modified;

                inventory.Status = "No Response";
                assign.Status = "No Response";
                assign.IsActive = false;
                try
                {
                    var email2 = new MailMessage
                    {
                        From = new MailAddress("DbnEventMangement@outlook.com"),
                        Subject = "No Response",
                        Body = $"Inventory Number: " + inventory.EventInventoryId + " \n\n" +
                               $"Hi {inventory.FirstName}, \n\n" +
                               $"Your inventory {inventory.EventInventoryId} could not be delivered, the driver tried to reach you at {inventory.Address} on {DateTime.Now.Date.ToLongDateString()} around {DateTime.Now.ToShortTimeString()} but there was no one to receive inventory.\n\n" +
                               "Your delivery will be rescheduled for another day.\n\n" +
                               "Regards,\r\nEventManagement Team"
                    };
                    var _event = db.Events.Find(inventory.EventId);
                    email2.To.Add(_event.EventMangerEmail);

                    var smtpClient = new SmtpClient();
                    smtpClient.Send(email2);
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Failed to send email due to, " + ex.Message;
                    return RedirectToAction("FinishInventoryDelivery");
                }

                db.SaveChanges();
            }
            else
            {
                TempData["ErrorMessage"] = "Something went Wrong Please try again later";
                return RedirectToAction("FinishInventoryDelivery");
            }

            TempData["ErrorMessage"] = "Inventory Delivery marked as no response from customer.";
            return RedirectToAction("MyAssignments");
        }

        // GET: DriverAssignments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DriverAssignment driverAssignment = db.DriverAssignments.Find(id);
            if (driverAssignment == null)
            {
                return HttpNotFound();
            }
            return View(driverAssignment);
        }

        // GET: DriverAssignments/Create
        public ActionResult Create(int id)
        {
            string InvId = " ";

            if (Session["InvId"] != null)
            {
                InvId = Session["InvId"] as string;
                ViewBag.Title = "Create";
            }
            else
            {
                TempData["ErrorMessage"] = "Sorry Your Session Ended.";
                return RedirectToAction("Index", "EventInventories");
            }

            int EventInventoryId = int.Parse(InvId);

            var Driver = db.Drivers.Find(id);
            var inventory = db.EventInventories.Find(EventInventoryId);
            DriverAssignment b = new DriverAssignment()
            {
                PreferredTime = inventory.PreferredTime,
                GenDeliveryDate = inventory.DeliveryDate.ToLongDateString(),
                EventInventoryId = EventInventoryId,
                DrivId = id,
                Email = Driver.Email
            };

            return View(b);
        }

        // POST: DriverAssignments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "AssDrivId,EventInventoryId,DrivId,Name,Surname,Email,Status,DeliveryDate,DeliveryTime,Created,GenDeliveryDate,preffaredTime")] DriverAssignment driverAssignment)
        {
            if (ModelState.IsValid)
            {
                var assign = db.DriverAssignments.Where(x => x.EventInventoryId == driverAssignment.EventInventoryId && x.DrivId == driverAssignment.DrivId).FirstOrDefault();
                if (assign == null)
                {
                    var assign2 = db.DriverAssignments.Where(x => x.EventInventoryId == driverAssignment.EventInventoryId).FirstOrDefault();
                    if (assign2 == null)
                    {
                        var driver = db.Drivers.Find(driverAssignment.DrivId);
                        var inventory = db.EventInventories.Find(driverAssignment.EventInventoryId);
                        inventory.Status = "Delivery Scheduled";
                        inventory.DriverEmail = driver.Email;
                        driverAssignment.Surname = driver.Surname;
                        driverAssignment.Name = driver.Name;
                        driverAssignment.Status = "Assigned";
                        driverAssignment.Created = DateTime.Now;
                        driverAssignment.IsActive = true;
                        Meths meth = new Meths();
                        int UniqueCode = Meths.GenerateRandomCode();
                        bool conflict = db.EventInventories.Where(x => x.UniqueCode == UniqueCode).Any();
                        while (conflict)
                        {
                            UniqueCode = Meths.GenerateRandomCode();
                            conflict = db.EventInventories.Where(x => x.UniqueCode == UniqueCode).Any();
                        }

                        // Generate QR code
                        QRCodeGenerator qrGenerator = new QRCodeGenerator();
                        QRCodeData qrCodeData = qrGenerator.CreateQrCode(UniqueCode.ToString(), QRCodeGenerator.ECCLevel.Q);
                        QRCode qrCode = new QRCode(qrCodeData);
                        Bitmap qrCodeImage = qrCode.GetGraphic(20); // Change the size of the QR code image as needed

                        // Convert the Bitmap to a byte array
                        using (MemoryStream stream = new MemoryStream())
                        {
                            qrCodeImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                            byte[] byteArr = stream.ToArray();

                            // Save the image to a folder
                            string fileName = $"{Guid.NewGuid().ToString()}.png";
                            string filePath = Path.Combine(Server.MapPath("~/images/"), fileName);
                            System.IO.File.WriteAllBytes(filePath, byteArr);

                            // Save the filename to the database
                            inventory.QrCodePicture = fileName;



                        }
                        inventory.UniqueCode = UniqueCode;



                        db.Entry(inventory).State = EntityState.Modified;
                        db.DriverAssignments.Add(driverAssignment);
                        try
                        {
                            var email = new MailMessage
                            {
                                From = new MailAddress("DbnEventMangement@outlook.com"),
                                Subject = "Delivery Assignment |  " + inventory.EventInventoryId,
                                Body = $"Delivery Date: " + driverAssignment.DeliveryDate + "\t\t Estimated Delivery Time: " + driverAssignment.DeliveryTime + " \n\n" +
                                       $"Hi {driver.Name}, \n\n" +
                                       $"Please note that, you have been assigned to a new delivery for inventory {inventory.EventInventoryId} to {inventory.Address}.\n\n" +
                                       "We'll email you the moment the inventory is ready for pickup.\n\n" +
                                       "Regards,\r\nEventManagement Team"
                            };
                            email.To.Add(User.Identity.Name);

                            var smtpClient = new SmtpClient();
                            smtpClient.Send(email);

                            var email2 = new MailMessage
                            {
                                From = new MailAddress("DbnEventMangement@outlook.com"),
                                Subject = "Delivery Scheduled |  " + inventory.EventInventoryId,
                                Body = $"Inventory Number: " + inventory.EventInventoryId + "\t\tDelivery Date: " + driverAssignment.DeliveryDate + "\t\t Estimated Delivery Time: " + driverAssignment.DeliveryTime + " \n\n" +
                                       $"Hi {inventory.FirstName}, \n\n" +
                                       $"Please note that, we’ve scheduled delivery for inventory {inventory.EventInventoryId}\n\n" +
                                       $"Your inventory is not out for delivery yet. It should arrive on {driverAssignment.DeliveryDate} at {driverAssignment.DeliveryTime}.\n\n" +
                                       "We'll email you the moment the delivery starts.\n\n" +
                                       "Regards,\r\nEventManagement Team"
                            };
                            email2.To.Add(User.Identity.Name);
                            smtpClient.Send(email2);
                        }
                        catch (Exception ex)
                        {
                            TempData["ErrorMessage"] = "Failed to send email due to, " + ex.Message;
                            return RedirectToAction("Index", "EventInventories");
                        }
                        db.SaveChanges();

                        TempData["SuccessMessage"] = "Driver Assigned Successfully";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        var driver = db.Drivers.Find(driverAssignment.DrivId);
                        var inventory = db.EventInventories.Find(driverAssignment.EventInventoryId);
                        inventory.Status = "Delivery Scheduled";
                        inventory.DriverEmail = driver.Email;
                        inventory.DeliveredBy = driverAssignment.DrivId;
                        inventory.IsDeliveryRescheduled = true;

                        driverAssignment.Surname = driver.Surname;
                        driverAssignment.Name = driver.Name;
                        driverAssignment.Status = "Assigned";
                        driverAssignment.Created = DateTime.Now;
                        driverAssignment.IsActive = true;
                        db.Entry(inventory).State = EntityState.Modified;
                        db.DriverAssignments.Add(driverAssignment);
                        try
                        {
                            var email = new MailMessage
                            {
                                From = new MailAddress("DbnEventMangement@outlook.com"),
                                Subject = "Delivery Assignment |  " + inventory.EventInventoryId,
                                Body = $"Delivery Date: " + driverAssignment.DeliveryDate + "\t\t Estimated Delivery Time: " + driverAssignment.DeliveryTime + " \n\n" +
                                       $"Hi {driver.Name}, \n\n" +
                                       $"Please note that, you have been assigned to a new delivery for inventory {inventory.EventInventoryId} to {inventory.Address}.\n\n" +
                                       "We'll email you the moment the inventory is ready for pickup.\n\n" +
                                       "Regards,\r\nEventManagement Team"
                            };
                            email.To.Add(User.Identity.Name);

                            var smtpClient = new SmtpClient();
                            smtpClient.Send(email);

                            var email2 = new MailMessage
                            {
                                From = new MailAddress("DbnEventMangement@outlook.com"),
                                Subject = "Delivery Rescheduled |  " + inventory.EventInventoryId,
                                Body = $"Inventory Number: " + inventory.EventInventoryId + "\t\tDelivery Date: " + driverAssignment.DeliveryDate + "\t\t Estimated Delivery Time: " + driverAssignment.DeliveryTime + " \n\n" +
                                       $"Hi {inventory.FirstName}, \n\n" +
                                       $"Please note that, we’ve rescheduled delivery for inventory {inventory.EventInventoryId}\n\n" +
                                       $"Your inventory is not out for delivery yet. It should arrive on {driverAssignment.DeliveryDate} at {driverAssignment.DeliveryTime}.\n\n" +
                                       "We'll email you the moment the delivery starts.\n\n" +
                                       "Regards,\r\nEventManagement Team"
                            };
                            email2.To.Add(User.Identity.Name);
                            smtpClient.Send(email2);
                        }
                        catch (Exception ex)
                        {
                            TempData["ErrorMessage"] = "Failed to send email due to, " + ex.Message;
                            return RedirectToAction("Index", "EventInventories");
                        }
                        db.SaveChanges();

                        TempData["SuccessMessage"] = "Driver Assigned Successfully";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    var driver = db.Drivers.Find(driverAssignment.DrivId);
                    var inventory = db.EventInventories.Find(driverAssignment.EventInventoryId);
                    inventory.Status = "Delivery Scheduled";
                    inventory.DriverEmail = driver.Email;
                    inventory.IsDeliveryRescheduled = true;
                    assign.Status = "Assigned";
                    assign.Created = DateTime.Now;
                    assign.IsActive = true;
                    db.Entry(inventory).State = EntityState.Modified;
                    db.Entry(assign).State = EntityState.Modified;

                    try
                    {
                        var email = new MailMessage
                        {
                            From = new MailAddress("DbnEventMangement@outlook.com"),
                            Subject = "Delivery Assignment |  " + inventory.EventInventoryId,
                            Body = $"Delivery Date: " + driverAssignment.DeliveryDate + "\t\t Estimated Delivery Time: " + driverAssignment.DeliveryTime + " \n\n" +
                                   $"Hi {driver.Name}, \n\n" +
                                   $"Please note that, you have been assigned to a new delivery for inventory {inventory.EventInventoryId} to {inventory.Address}.\n\n" +
                                   "We'll email you the moment the inventory is ready for pickup.\n\n" +
                                   "Regards,\r\nEventManagement Team"
                        };
                        email.To.Add(User.Identity.Name);

                        var smtpClient = new SmtpClient();
                        smtpClient.Send(email);

                        var email2 = new MailMessage
                        {
                            From = new MailAddress("DbnEventMangement@outlook.com"),
                            Subject = "Delivery Rescheduled |  " + inventory.EventInventoryId,
                            Body = $"Inventory Number: " + inventory.EventInventoryId + "\t\tDelivery Date: " + driverAssignment.DeliveryDate + "\t\t Estimated Delivery Time: " + driverAssignment.DeliveryTime + " \n\n" +
                                   $"Hi {inventory.FirstName}, \n\n" +
                                   $"Please note that, we’ve rescheduled delivery for inventory {inventory.EventInventoryId}\n\n" +
                                   $"Your inventory is not out for delivery yet. It should arrive on {driverAssignment.DeliveryDate} at {driverAssignment.DeliveryTime}.\n\n" +
                                   "We'll email you the moment the delivery starts.\n\n" +
                                   "Regards,\r\nEventManagement Team"
                        };
                        email2.To.Add(User.Identity.Name);
                        smtpClient.Send(email2);
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = "Failed to send email due to, " + ex.Message;
                        return RedirectToAction("Index", "EventInventories");
                    }
                    db.SaveChanges();
                    Session["InvId"] = null;
                    TempData["SuccessMessage"] = "Driver Assigned Successfully";
                    return RedirectToAction("Index");
                }
            }

            return View(driverAssignment);
        }

        // GET: DriverAssignments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DriverAssignment driverAssignment = db.DriverAssignments.Find(id);
            if (driverAssignment == null)
            {
                return HttpNotFound();
            }
            ViewBag.Id = new SelectList(db.EventInventories, "Id", "FirstName", driverAssignment.EventInventoryId);
            ViewBag.DrivId = new SelectList(db.Drivers, "DrivId", "Name", driverAssignment.DrivId);
            return View(driverAssignment);
        }

        // POST: DriverAssignments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AssDrivId,Id,DrivId,Name,Surname,Email,Status,DeliveryDate,DeliveryTime,Created")] DriverAssignment driverAssignment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(driverAssignment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Id = new SelectList(db.EventInventories, "Id", "FirstName", driverAssignment.EventInventoryId);
            ViewBag.DrivId = new SelectList(db.Drivers, "DrivId", "Name", driverAssignment.DrivId);
            return View(driverAssignment);
        }

        // GET: DriverAssignments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DriverAssignment driverAssignment = db.DriverAssignments.Find(id);
            if (driverAssignment == null)
            {
                return HttpNotFound();
            }
            return View(driverAssignment);
        }

        // POST: DriverAssignments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            DriverAssignment driverAssignment = db.DriverAssignments.Find(id);
            db.DriverAssignments.Remove(driverAssignment);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

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
