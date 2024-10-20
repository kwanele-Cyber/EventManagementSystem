using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EventMangementSystem.Models;
using iTextSharp.text.pdf;
using iTextSharp.text;

namespace EventMangementSystem.Controllers
{
    public class DamageReportsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: DamageReports
        public ActionResult Index()
        {
            var damageReports = db.DamageReports.Include(d => d.Event).Include(d => d.Inventory);
            return View(damageReports.ToList());
        }

        // GET: DamageReports/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DamageReport damageReport = db.DamageReports.Find(id);
            if (damageReport == null)
            {
                return HttpNotFound();
            }
            return View(damageReport);
        }

        // GET: DamageReports/Create
        public ActionResult Create(int? InventoryId, int? eventId)
        {
            ViewBag.EventId = eventId;
            ViewBag.EquipmentId = InventoryId;
            return View();
        }

        // POST: DamageReports/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ReportId,EquipmentId,DamageDescription,ReportDate,EventId,TotalCost")] DamageReport damageReport, int damagedItemsCount, int? InventoryId ,int? eventId)
        {
            if (ModelState.IsValid)
            {
                var itemget = db.Inventories.Find(InventoryId);
                var id = itemget.InventoryId;
                var Pricetofix = itemget.PriceToService;
                var PricetoReplace = itemget.PriceToRelace;
                if (damageReport.DamageDescription == "Minor")
                {
                    damageReport.TotalCost = damagedItemsCount * Pricetofix;

                }
                else if (damageReport.DamageDescription == "Moderate") 
                {
                    damageReport.TotalCost = damagedItemsCount * Pricetofix * 2;

                }
                else if (damageReport.DamageDescription == "Severe") 
                {
                    damageReport.TotalCost = damagedItemsCount * Pricetofix * 3;

                }
                else 
                {
                    damageReport.TotalCost = PricetoReplace;

                }

                damageReport.EquipmentId = id;
                damageReport.ReportDate = DateTime.Now;

                db.DamageReports.Add(damageReport);
                db.SaveChanges();
                return RedirectToAction("ViewQuote", new { id = damageReport.ReportId});
            }

            ViewBag.EventId = new SelectList(db.Events, "EventId", "Name", damageReport.EventId);
            ViewBag.EquipmentId = new SelectList(db.Inventories, "InventoryId", "ItemName", damageReport.EquipmentId);
            return View(damageReport);
        }











        //finally

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Finally(DamageReport damageReport)
        {
            if (ModelState.IsValid)
            {
                

                db.DamageReports.Add(damageReport);
                db.SaveChanges();
                return RedirectToAction("details", new { id = damageReport.ReportId });
            }
            return HttpNotFound();
        }




        public ActionResult ViewQuote(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DamageReport damageReport = db.DamageReports.Find(id);
            if (damageReport == null)
            {
                return HttpNotFound();
            }


            ViewBag.BareCost = damageReport.TotalCost * 0.85;
            ViewBag.Vat = damageReport.TotalCost * 0.15;


            return View(damageReport);

           // return RedirectToAction("CreatePdf", new { id = damageReport.ReportId });
        }


















        public ActionResult CreatePdf(int id)
        {
            // Retrieve your DamageReport model by ID or however you store it
            var damageReport = db.DamageReports.Find(id); // Implement this method to get your data



            // Create a PDF document
            using (MemoryStream stream = new MemoryStream())
            {
                Document pdfDoc = new Document(PageSize.A4);
                PdfWriter.GetInstance(pdfDoc, stream);
                pdfDoc.Open();

                // Add content to the PDF
                pdfDoc.Add(new Paragraph("Damage Report"));
                pdfDoc.Add(new Paragraph($"Please take note of the following deductions for the {damageReport.Event.Name} Event"));
                pdfDoc.Add(new Paragraph($"Damage Type: {damageReport.DamageDescription}"));

                // Format the total cost as South African Rand (ZAR)
                string formattedCost = string.Format("R {0:0.00}", damageReport.TotalCost);
                pdfDoc.Add(new Paragraph($"Total Cost: {formattedCost}"));

                // Close the document
                pdfDoc.Close();
                return File(stream.ToArray(), "application/pdf", "DamageReport.pdf");
            }
        }



















        // GET: DamageReports/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DamageReport damageReport = db.DamageReports.Find(id);
            if (damageReport == null)
            {
                return HttpNotFound();
            }
            ViewBag.EventId = new SelectList(db.Events, "EventId", "Name", damageReport.EventId);
            ViewBag.EquipmentId = new SelectList(db.Inventories, "InventoryId", "ItemName", damageReport.EquipmentId);
            return View(damageReport);
        }

        // POST: DamageReports/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ReportId,EquipmentId,DamageDescription,ReportDate,EventId,TotalCost")] DamageReport damageReport)
        {
            if (ModelState.IsValid)
            {
                db.Entry(damageReport).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.EventId = new SelectList(db.Events, "EventId", "Name", damageReport.EventId);
            ViewBag.EquipmentId = new SelectList(db.Inventories, "InventoryId", "ItemName", damageReport.EquipmentId);
            return View(damageReport);
        }

        // GET: DamageReports/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DamageReport damageReport = db.DamageReports.Find(id);
            if (damageReport == null)
            {
                return HttpNotFound();
            }
            return View(damageReport);
        }

        // POST: DamageReports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            DamageReport damageReport = db.DamageReports.Find(id);
            db.DamageReports.Remove(damageReport);
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
