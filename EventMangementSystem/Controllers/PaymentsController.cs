using EventMangementSystem.Models;
using Microsoft.AspNet.Identity;
using PayPal.Api;
using Stripe;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace EventMangementSystem.Controllers
{
    public class PaymentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [HttpPost]
        public ActionResult CreateDamagePayment(decimal totalCost, int reportId, string refund= "NO")
        {
            var damageReport = db.DamageReports
                .Include("ReturnProcess")
                .FirstOrDefault(t => t.ReportId == reportId);

            if(damageReport == null)
            {
                return HttpNotFound("Invalid Damage Report Index");
            }

            if(damageReport.IsPaid || damageReport.ReturnProcess?.Status == "Settled")
            {

                // Handle the payment cancellation
                TempData["ErrorMessage"] = "Payment has already been settled.";
                return RedirectToAction("ReturnedEquipment", "DriverAssignments");
            }

            Session["ID"] = reportId.ToString();
            Session["Refund"] = refund;

            var CurrentUser = User.Identity.Name;
            double convertedTot = Math.Round((double)totalCost / 18.904);
            int Rem = (int)((double)totalCost % 18.904);
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

                        description = $"DamageReport Payment for {reportId}"
                    }
                },
                redirect_urls = new RedirectUrls
                {
                    return_url = Url.Action("CompleteDamagePayment", "Payments", null, Request.Url.Scheme),
                    cancel_url = Url.Action("CancelDamagePayment", "Payments", null, Request.Url.Scheme)
                }
            };

            // Create the payment and get the approval URL
            var createdPayment = payment.Create(apiContext);
            var approvalUrl = createdPayment.links.FirstOrDefault(l => l.rel == "approval_url")?.href;

            // Redirect the user to the PayPal approval URL
            return Redirect(approvalUrl);

        }

        public ActionResult CompleteDamagePayment(string paymentId, string token, string PayerID)
        {
            // Set up the PayPal API context
            var apiContext = PayPalConfig.GetAPIContext();

            // Find the related damage report and mark as paid
            int reportId = int.Parse(Session["ID"].ToString());
            var damageReport = db.DamageReports
                .Include("ReturnProcess")
                .FirstOrDefault(t => t.ReportId == reportId);

            // Execute the payment
            var paymentExecution = new PaymentExecution { payer_id = PayerID };
            var executedPayment = new Payment { id = paymentId }.Execute(apiContext, paymentExecution);

            if (damageReport != null)
            {
                damageReport.IsPaid = true;
                damageReport.ReturnProcess.Status = "Settled";
                db.Entry(damageReport).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }

            // Redirect the user to a success page
            return RedirectToAction("PaymentSuccess", "Payments");
        }

        public ActionResult CancelDamagePayment()
        {
            // Handle the payment cancellation
            TempData["ErrorMessage"] = "Payment was cancelled.";
            return RedirectToAction("ReturnedEquipment", "DriverAssignments");
        }

        public ActionResult PaymentSuccess()
        {
            TempData["SuccessMessage"] = "Payment completed successfully.";
            return RedirectToAction("ReturnedEquipment", "DriverAssignments");
        }
    }


}
