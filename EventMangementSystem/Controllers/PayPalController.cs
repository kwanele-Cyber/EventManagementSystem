using EventMangementSystem.Models;
using PayPal.Api;
using Stripe.Climate;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using System.Net.Sockets;
using System.Web;
using System.Web.Mvc;

namespace EventMangementSystem.Controllers
{
    public class PayPalController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult CreatePayment(double CartTotal,int? id, string refund="NO")
        { 
            
            Session["ID"] = id.ToString();
            Session["Refund"] = refund;
           
            var CurrentUser = User.Identity.Name;
            double convertedTot = Math.Round(CartTotal / 18.904);
            int Rem = (int)(CartTotal % 18.904);
            string Cost = convertedTot.ToString() + "." + Rem;

            // Set up the PayPal API context
            var apiContext = PayPalConfig.GetAPIContext();

            // Retrieve the API credentials from configuration
            var clientId = ConfigurationManager.AppSettings["PayPalClientId"];
            var clientSecret = ConfigurationManager.AppSettings["PayPalClientSecret"];
            apiContext.Config = new Dictionary<string, string> { { "mode", "sandbox" } };
            var accessToken = new OAuthTokenCredential(clientId, clientSecret, apiContext.Config).GetAccessToken();
            apiContext.AccessToken = accessToken;

            // Create a new payment object
            var payment = new Payment
            {
                intent = "sale",
                payer = new Payer { payment_method = "paypal" },
                transactions = new List<Transaction>
                {
            new Transaction
            {
                amount = new Amount
                {

                    total = Cost,
                    currency = "USD"
                },

                description = "Shop Payment"
            }
        },
                redirect_urls = new RedirectUrls
                {
                    return_url = Url.Action("CompletePayment", "PayPal", null, Request.Url.Scheme),
                    cancel_url = Url.Action("CancelPayment", "PayPal", null, Request.Url.Scheme)
                }
            };

            // Create the payment and get the approval URL
            var createdPayment = payment.Create(apiContext);
            var approvalUrl = createdPayment.links.FirstOrDefault(l => l.rel == "approval_url")?.href;

            // Redirect the user to the PayPal approval URL
            return Redirect(approvalUrl);

        }


        public ActionResult CompletePayment(string paymentId, string token, string PayerID)
        {
            // Set up the PayPal API context
            var apiContext = PayPalConfig.GetAPIContext();

            // Execute the payment
            var paymentExecution = new PaymentExecution { payer_id = PayerID };
            var executedPayment = new Payment { id = paymentId }.Execute(apiContext, paymentExecution);

            // Process the payment completion
            // You can save the transaction details or perform other necessary actions

            // Redirect the user to a success page
            return RedirectToAction("PaymentSuccess");
        }

        public ActionResult CancelPayment()
        {
            // Handle the payment cancellation
            // You can redirect the user to a cancellation page or perform other necessary actions

            // Redirect the user to a cancellation page
            return RedirectToAction("PaymentCancelled");
        }

        public ActionResult PaymentSuccess()
        {

            string ID;
            string refund;
            if(Session["ID"] != null)
            {
                
                ID = Session["ID"] as string;
                refund = Session["Refund"] as string;

                if (refund == "NO") {

                    int id = int.Parse(ID);
                    var ticket = db.Tickets.Find(id);
                    try
                    {
                        // Prepare email message
                        var email2 = new MailMessage();
                        email2.From = new MailAddress("DbnEventMangement@outlook.com");
                        email2.To.Add(User.Identity.Name);
                        email2.Subject = "Payment Confirmation |  " + id;
                        string emailBody = $"Ticket Number: " + id + " \n\n" +
                        $"Hi {ticket.AttendeeName}, \n\n" +
                       $"Thank you, we’ve received your payment for event {ticket.Event.Name}\n\n" +

                       $"Regards,\r\nEvent Mangement";
                        email2.Body = emailBody;


                        var smtpClient = new SmtpClient();

                        smtpClient.Send(email2);

                    }
                    catch (Exception ex)
                    {
                        db.Tickets.Remove(ticket);
                        db.SaveChanges();

                        TempData["ErrorMessage"] = "Failed to send email due to, " + ex.Message;

                        return RedirectToAction("Purchase", "Event", new { id = ticket.EventId });
                    }
                    
                    if(ticket.Quantity>1)
                    {
                        ticket.TotalPrice = ticket.TotalPrice / ticket.Quantity;
                        db.Entry(ticket).State = EntityState.Modified;
                        db.SaveChanges();
                        while (ticket.Quantity > 1)
                        {
                            Ticket tick = new Ticket()
                            {
                                AttendeeName = ticket.AttendeeName,
                                EventId = ticket.EventId,
                                Quantity = 1,
                                TotalPrice= ticket.TotalPrice,
                                AttendeeEmail = ticket.AttendeeEmail,
                                CheckInCode = Guid.NewGuid().ToString()
                        };
                            db.Tickets.Add(tick);

                            ticket.Quantity--;
                            db.SaveChanges();
                        }
                    }

                    
                    TempData["SuccessMessage"] = "Ticket Purchased Successfully Thank you!!";
                    return RedirectToAction("Details", "Ticket", new { id = id });
                }
                else if(refund == "Donation")
                {
                    int id = int.Parse(ID);
                    var donation = db.Donations.Find(id);
                    try
                    {
                        var email2 = new MailMessage();
                        email2.From = new MailAddress("DbnEventMangement@outlook.com");
                        email2.To.Add(User.Identity.Name);
                        email2.Subject = "Payment Confirmation |  " + id;
                        string emailBody = $"Donation Number: " + id + " \n\n" +
                        $"Hi {donation.Name}, \n\n" +
                       $"Thank you, we’ve received your donation of {donation.Amount} for event {donation.Event.Name}\n\n" +

                       $"Regards,\r\nEvent Mangement";
                        email2.Body = emailBody;


                        var smtpClient = new SmtpClient();

                        smtpClient.Send(email2);

                    }
                    catch (Exception ex)
                    {
                        db.Donations.Remove(donation);
                        db.SaveChanges();

                        TempData["ErrorMessage"] = "Failed to send email due to, " + ex.Message;

                        return RedirectToAction("Create", "Donations", new { id = donation.EventId });
                    }
                    TempData["SuccessMessage"] = "Donation completed Successfully Thank you!!";
                    return RedirectToAction("Details", "Event", new { id = donation.EventId });
                }
                else
                {
                    int id = int.Parse(ID);
                    var ticket = db.Tickets.Find(id);
                    ticket.Refunded = true;
                    db.Entry(ticket).State = EntityState.Modified;

                    try
                    {
                        // Prepare email message
                        var email2 = new MailMessage();
                        email2.From = new MailAddress("DbnEventMangement@outlook.com");
                        email2.To.Add(User.Identity.Name);
                        email2.Subject = "Refund Confirmation |  " + id;
                        string emailBody = $"Ticket Number: " + id + " \n\n" +
                        $"Hi {ticket.AttendeeName}, \n\n" +
                       $"An amounnt of {ticket.TotalPrice} has been refunded to your account, for event {ticket.Event.Name}\n\n" +

                       $"Regards,\r\nEvent Mangement";
                        email2.Body = emailBody;


                        var smtpClient = new SmtpClient();

                        smtpClient.Send(email2);

                    }
                    catch (Exception ex)
                    {

                        ticket.Refunded = false;
                        db.Entry(ticket).State = EntityState.Modified;
                        db.SaveChanges();

                        TempData["ErrorMessage"] = "Failed to send email due to, " + ex.Message;

                        return RedirectToAction("PayAttendees", "Event", new { id = ticket.EventId });
                    }
                    db.SaveChanges();
                    return RedirectToAction("PayAttendees", "Event", new { id = ticket.EventId });
                }
            }
            else
            {

                TempData["ErrorMessage"] = "Sorry your session Ended";
                return RedirectToAction("Index", "Home");

            }
                
           

        }

    }

}