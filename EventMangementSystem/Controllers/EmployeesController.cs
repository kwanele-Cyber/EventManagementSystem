using System;
using System.Linq;
using System.Web.Mvc;
using EventMangementSystem.Models;
using EventMangementSystem.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace EventMangementSystem.Controllers
{
    public class EmployeesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Employees/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EmployeeViewModel model)
        {
            model.Role = "Employee";

            if (ModelState.IsValid)
            {
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

                // Ensure Role exists
                if (!roleManager.RoleExists(model.Role))
                {
                    roleManager.Create(new IdentityRole(model.Role));
                }

                //Get Employeer Info...
                var employerEmail = User.Identity.GetUserName();
                int employerId = 0;

                if (User.IsInRole(nameof(RoleEnum.ServiceProvider)))
                {
                    employerId = db.ServiceProviders.FirstOrDefault(sp => sp.email == employerEmail)?.Id ?? 0;
                    
                }
                else
                {
                    throw new Exception("You don't have enought permision to do this action, Only Service Providers can add Employees");
                }
                 

                // Create ApplicationUser (Identity User)
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Name = model.Name,
                    EmailConfirmed = true,
                };

                var userCreationResult = userManager.Create(user, model.Password);

                if (userCreationResult.Succeeded)
                {
                    // Add the new user to the specified role
                    userManager.AddToRole(user.Id, nameof(RoleEnum.Employee));

                    // Create Employee record
                    var employee = new Employee
                    {
                        Name = model.Name,
                        Email = model.Email,
                        Position = model.Position,
                        DateHired = model.DateHired,
                        ServiceProviderId = employerId
                    };

                    db.Employees.Add(employee);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Employee created successfully!";
                    return RedirectToAction("Index", "Employees");
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to create user. " + string.Join(", ", userCreationResult.Errors);
                }
            }

            return View(model);
        }

        // GET: Employees/Index
        public ActionResult Index()
        {
            var employees = db.Employees.ToList();
            return View(employees);
        }


        // GET: Employees/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            return View(employee);
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Employee employee, string redirectUrl = null)
        {
            if (ModelState.IsValid)
            {
                db.Entry(employee).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                // If redirectUrl is not null or empty, redirect there. Otherwise, fallback to a default redirection
                if (!string.IsNullOrEmpty(redirectUrl))
                {
                    return Redirect(redirectUrl);
                }

                return RedirectToAction("Index", "Employees"); // Default fallback to Index action
            }
            ViewBag.RedirectUrl = redirectUrl;
            return View(employee);
        }

        // GET: Employees/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, string redirectUrl = null)
        {
            Employee employee = db.Employees.Find(id);
            db.Employees.Remove(employee);
            db.SaveChanges();

            // If redirectUrl is not null or empty, redirect there. Otherwise, fallback to a default redirection
            if (!string.IsNullOrEmpty(redirectUrl))
            {
                return Redirect(redirectUrl);
            }

            return RedirectToAction("Index", "Employees"); // Default fallback to Index action
        }

        // GET: Employees/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }

            return View(employee);
        }

    }
}
