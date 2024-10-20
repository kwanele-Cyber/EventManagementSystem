using EventMangementSystem.Models;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web;
using System;
using System.IO;

namespace EventMangementSystem.Controllers
{
    

    public class InventoryController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();


        // GET: Inventory/Add
        public ActionResult Add(int eventId)
        {
            ViewBag.InventoryId = new SelectList(db.Inventories, "InventoryId", "ItemName");
            var eventInventory = new EventInventory { EventId = eventId };
            return View(eventInventory);
        }

        // POST: Inventory/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add([Bind(Include = "EventId,InventoryId,QuantityRequired,Address,Status,Email")] EventInventory eventInventory)
        {
            if (ModelState.IsValid)
            {
                db.EventInventories.Add(eventInventory);
                db.SaveChanges();
                return RedirectToAction("Index", "Event"); // Redirect to the event list or another relevant page
            }

            ViewBag.InventoryId = new SelectList(db.Inventories, "InventoryId", "ItemName", eventInventory.InventoryId);
            return View(eventInventory);
        }
        // GET: Inventory
        public ActionResult Index()
        {
            return View(db.Inventories.ToList());
        }

        // GET: Inventory/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Inventory inventory = db.Inventories.Find(id);
            if (inventory == null)
            {
                return HttpNotFound();
            }
            return View(inventory);
        }

        // GET: Inventory/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Inventory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "InventoryId,ItemName,Description,QuantityAvailable,PriceToService,PriceToRelace")] Inventory inventory, HttpPostedFileBase pictureFile)
        {
            if (ModelState.IsValid)
            {
                
                string pictureFileName = Guid.NewGuid().ToString() + Path.GetExtension(pictureFile.FileName);
                string picturePath = Path.Combine(Server.MapPath("~/assets/images/"), pictureFileName);
                pictureFile.SaveAs(picturePath);

                // Set the picture path in the record
                inventory.picture = pictureFileName;

                db.Inventories.Add(inventory);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(inventory);
        }

        // GET: Inventory/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Inventory inventory = db.Inventories.Find(id);
            if (inventory == null)
            {
                return HttpNotFound();
            }
            return View(inventory);
        }

        // POST: Inventory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "InventoryId,ItemName,Description,QuantityAvailable")] Inventory inventory)
        {
            if (ModelState.IsValid)
            {
                db.Entry(inventory).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(inventory);
        }

        // GET: Inventory/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Inventory inventory = db.Inventories.Find(id);
            if (inventory == null)
            {
                return HttpNotFound();
            }
            return View(inventory);
        }

        // POST: Inventory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Inventory inventory = db.Inventories.Find(id);
            db.Inventories.Remove(inventory);
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