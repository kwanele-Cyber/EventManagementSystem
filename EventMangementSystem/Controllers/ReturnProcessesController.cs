using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EventMangementSystem.Models;
using Org.BouncyCastle.Operators.Utilities;
using Rotativa;

namespace EventMangementSystem.Controllers
{
    public class ReturnProcessesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ReturnProcess returnProcess = db.ReturnProcesses.Find(id);
            if (returnProcess == null)
            {
                return HttpNotFound();
            }
            return View(returnProcess);
        }

        
        // GET: ReturnProcesses/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ReturnProcess returnProcess = db.ReturnProcesses.Find(id);
            if (returnProcess == null)
            {
                return HttpNotFound();
            }
            ViewBag.DriverAssignmentId = new SelectList(db.DriverAssignments, "AssDrivId", "Name", returnProcess.DriverAssignmentId);
            ViewBag.EventInventoryId = new SelectList(db.EventInventories, "EventInventoryId", "DriverSignature", returnProcess.EventInventoryId);
            return View(returnProcess);
        }







        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerateInvoice(int returnProcessId)
        {
            if (returnProcessId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            DamageReport damageReport = db.DamageReports
            .FirstOrDefault(dr => dr.findRecord == returnProcessId);
            ViewBag.ReturnProcessId = returnProcessId;

            if (damageReport == null)
            {
                return HttpNotFound();
            }

            double total = 0;

            //Must calculate total cost first 
            var FindMissing = (damageReport.ReturnProcess.EventInventory.QuantityRequired) -(damageReport.ReturnProcess.QuantityReturned);
            if (damageReport.DamageDescription == "Bad")
            {
                total = FindMissing * damageReport.Inventory.PriceToRelace;


            }

            else
            {
                 total = FindMissing * damageReport.Inventory.PriceToService;

            }
            // Populate the view model
            DamageReportViewModel damageReportViewModel = new DamageReportViewModel
            {
                ReportId = damageReport.ReportId,
                EquipmentId = damageReport.EquipmentId,
                DamageDescription = damageReport.DamageDescription,
                ReportDate = damageReport.ReportDate,
                EventId = damageReport.EventId,
                TotalCost = total,
                bareCost = total * 0.85,
                vat = total * 0.15,
                Inventory = damageReport.Inventory,
                Event = damageReport.Event
            };

            return View(damageReportViewModel);
        }

        [HttpPost]
        public ActionResult DownloadPDF(int returnProcessId)
        {
            ViewBag.ReturnProcessId = returnProcessId;
            if (returnProcessId == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Retrieve the damage report from the database using returnProcessId
            DamageReport damageReportEntity = db.DamageReports
                .FirstOrDefault(dr => dr.findRecord == returnProcessId);

            if (damageReportEntity == null)
            {
                return HttpNotFound();
            }

            // Calculate the total cost
            double total = 0;
            var missingQuantity = damageReportEntity.ReturnProcess.EventInventory.QuantityRequired - damageReportEntity.ReturnProcess.QuantityReturned;

            if (damageReportEntity.DamageDescription == "Bad")
            {
                total = missingQuantity * damageReportEntity.Inventory.PriceToRelace;
            }
            else
            {
                total = missingQuantity * damageReportEntity.Inventory.PriceToService;
            }

            // Prepare the view model
            DamageReportViewModel damageReportViewModel = new DamageReportViewModel
            {
                ReportId = damageReportEntity.ReportId,
                EquipmentId = damageReportEntity.EquipmentId,
                DamageDescription = damageReportEntity.DamageDescription,
                ReportDate = damageReportEntity.ReportDate,
                EventId = damageReportEntity.EventId,
                TotalCost = total,
                bareCost = total * 0.85,
                vat = total * 0.15,
                Inventory = damageReportEntity.Inventory,
                Event = damageReportEntity.Event
            };

            // Set the PDF flag to skip the layout
            ViewBag.IsPdf = true;

            // Use the "GenerateInvoice" view to generate the PDF
            return new ViewAsPdf("GenerateInvoice", damageReportViewModel)
            {
                FileName = "DamageReportInvoice.pdf",

                // Optionally, specify custom options like margins, page size, etc.
                PageSize = Rotativa.Options.Size.A4,
                CustomSwitches = "--disable-smart-shrinking --no-stop-slow-scripts"
            };
        }





        // GET: ReturnProcesses/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ReturnProcess returnProcess = db.ReturnProcesses.Find(id);
            if (returnProcess == null)
            {
                return HttpNotFound();
            }
            return View(returnProcess);
        }

        // POST: ReturnProcesses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ReturnProcess returnProcess = db.ReturnProcesses.Find(id);
            db.ReturnProcesses.Remove(returnProcess);
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
