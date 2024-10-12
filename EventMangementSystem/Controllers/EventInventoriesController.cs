using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EventMangementSystem.Models;

namespace EventMangementSystem.Controllers
{
    public class EventInventoriesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: EventInventories
        public ActionResult Index()
        {
            var eventInventories = db.EventInventories.Include(e => e.Event);
            return View(eventInventories.ToList());
        }

        // GET: EventInventories/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EventInventory eventInventory = db.EventInventories.Find(id);
            if (eventInventory == null)
            {
                return HttpNotFound();
            }
            return View(eventInventory);
        }

        // GET: EventInventories/Create
        public ActionResult Create()
        {
            ViewBag.EventId = new SelectList(db.Events, "EventId", "Name");
            return View();
        }

        // POST: EventInventories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "EventInventoryId,EventId,InventoryId,QuantityRequired,UniqueCode,DriverSignature,AdminSignature,Status,Email,QrCodePicture,Address,DriverEmail,FirstName,PreferredTime,DeliveredBy,DeliveryDate,DeliveredOn,IsDeliveryRescheduled")] EventInventory eventInventory)
        {
            if (ModelState.IsValid)
            {
                db.EventInventories.Add(eventInventory);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.EventId = new SelectList(db.Events, "EventId", "Name", eventInventory.EventId);
            return View(eventInventory);
        }

        // GET: EventInventories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EventInventory eventInventory = db.EventInventories.Find(id);
            if (eventInventory == null)
            {
                return HttpNotFound();
            }
            ViewBag.EventId = new SelectList(db.Events, "EventId", "Name", eventInventory.EventId);
            return View(eventInventory);
        }

        // POST: EventInventories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "EventInventoryId,EventId,InventoryId,QuantityRequired,UniqueCode,DriverSignature,AdminSignature,Status,Email,QrCodePicture,Address,DriverEmail,FirstName,PreferredTime,DeliveredBy,DeliveryDate,DeliveredOn,IsDeliveryRescheduled")] EventInventory eventInventory)
        {
            if (ModelState.IsValid)
            {
                db.Entry(eventInventory).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.EventId = new SelectList(db.Events, "EventId", "Name", eventInventory.EventId);
            return View(eventInventory);
        }

        // GET: EventInventories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EventInventory eventInventory = db.EventInventories.Find(id);
            if (eventInventory == null)
            {
                return HttpNotFound();
            }
            return View(eventInventory);
        }

        // POST: EventInventories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            EventInventory eventInventory = db.EventInventories.Find(id);
            db.EventInventories.Remove(eventInventory);
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
