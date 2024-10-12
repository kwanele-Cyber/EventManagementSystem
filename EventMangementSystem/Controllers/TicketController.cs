using EventMangementSystem.Models;
using QRCoder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace EventMangementSystem.Controllers
{
    public class TicketController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Ticket/Details/5
        public ActionResult Details(int id)
        {
            var ticket = db.Tickets.Include(t => t.Event).FirstOrDefault(t => t.TicketId == id);
            if (ticket == null)
            {
                return HttpNotFound();
            }

            ticket.QRCode = GenerateQRCode(ticket.CheckInCode);
            return View(ticket);
        }
        public ActionResult Tickets()
        {
            var ticket = db.Tickets.Where(x=>x.AttendeeEmail == User.Identity.Name).ToList();
           

            
            return View(ticket);
        }
        private string GenerateQRCode(string text)
        {
            using (var qrGenerator = new QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new QRCode(qrCodeData))
            using (var bitmap = qrCode.GetGraphic(20))
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return Convert.ToBase64String(stream.ToArray());
            }
        }
    }
}