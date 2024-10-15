using EventMangementSystem.Models;
using EventMangementSystem.ViewModels;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace EventMangementSystem.Controllers
{
    public class TeamsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: CreateTeam
        public ActionResult CreateTeam()
        {
            var currentUserEmail = User.Identity.GetUserName();
            var serviceProvider = db.ServiceProviders.FirstOrDefault(sp => sp.email == currentUserEmail);

            if (serviceProvider == null)
            {
                return HttpNotFound("Service provider not found.");
            }

            var employees = db.Employees
                .Where(e => e.ServiceProviderId == serviceProvider.Id)
                .ToList();

            var viewModel = new TeamEmployeeViewModel
            {
                Employees = employees, // Populate the list of employees
                Team = new Team() // Empty team model to be filled by the user
            };

            return View(viewModel);
        }

        // POST: CreateTeam
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateTeam(TeamEmployeeViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                db.Teams.Add(viewModel.Team);
                db.SaveChanges();

                // Assign selected employees to the newly created team
                foreach (var employeeId in viewModel.SelectedEmployeeIds)
                {
                    var employee = db.Employees.Find(employeeId);
                    if (employee != null)
                    {
                        var teamMember = new TeamMember
                        {
                            TeamId = viewModel.Team.TeamId,
                            EmployeeId = employee.EmployeeId,
                            Role = "Team Member" // Default role, can be customized
                        };
                        db.TeamMembers.Add(teamMember);
                    }
                }

                db.SaveChanges();
                // Redirect to Edit action after creating the team
                return RedirectToAction("Edit", new { id = viewModel.Team.TeamId });
            }

            // Reload the employees list in case of an error
            var currentUserEmail = User.Identity.GetUserName();
            var serviceProvider = db.ServiceProviders.FirstOrDefault(sp => sp.email == currentUserEmail);
            viewModel.Employees = db.Employees
                .Where(e => e.ServiceProviderId == serviceProvider.Id)
                .ToList();

            return View(viewModel);
        }

        // GET: Team/Index
        public ActionResult Index()
        {
            var email = User.Identity.GetUserName();
            List<Team> teams = new List<Team>();

           
            teams = db.Teams
                .Include(t => t.GroupTasks)
                .Include(t => t.TeamMembers.Select(tm => tm.Employee))
                .ToList();
            
            return View(teams);
        }

        // GET: Team/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Team team = db.Teams.Include(t => t.TeamMembers.Select(tm => tm.Employee))
                                .FirstOrDefault(t => t.TeamId == id);
            if (team == null)
            {
                return HttpNotFound();
            }

            // Fetch employees for selection if editing team members
            var employees = db.Employees.ToList();
            var viewModel = new TeamEmployeeViewModel
            {
                Team = team,
                Employees = employees,
                SelectedEmployeeIds = team.TeamMembers.Select(tm => tm.EmployeeId).ToList()
            };

            return View(viewModel);
        }

        // POST: Team/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(TeamEmployeeViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                db.Entry(viewModel.Team).State = EntityState.Modified;

                // Clear existing team members
                var existingMembers = db.TeamMembers.Where(tm => tm.TeamId == viewModel.Team.TeamId).ToList();
                db.TeamMembers.RemoveRange(existingMembers);

                // Reassign selected employees
                foreach (var employeeId in viewModel.SelectedEmployeeIds)
                {
                    var employee = db.Employees.Find(employeeId);
                    if (employee != null)
                    {
                        var teamMember = new TeamMember
                        {
                            TeamId = viewModel.Team.TeamId,
                            EmployeeId = employee.EmployeeId,
                            Role = "Team Member" // Default role, can be customized
                        };
                        db.TeamMembers.Add(teamMember);
                    }
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Reload employees and selected members in case of validation errors
            var employees = db.Employees.ToList();
            viewModel.Employees = employees;
            return View(viewModel);
        }

        // GET: Team/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Team team = db.Teams.Find(id);
            if (team == null)
            {
                return HttpNotFound();
            }
            return View(team);
        }

        // POST: Team/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Team team = db.Teams.Find(id);

            // Remove all team members associated with the team
            var teamMembers = db.TeamMembers.Where(tm => tm.TeamId == id).ToList();
            db.TeamMembers.RemoveRange(teamMembers);

            db.Teams.Remove(team);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Teams/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Team team = db.Teams
                .Include(t => t.GroupTasks)
                .Include(t => t.TeamMembers.Select(tm => tm.Employee))
                .FirstOrDefault(t => t.TeamId == id);

            if (team == null)
            {
                return HttpNotFound();
            }


            // Assuming `GroupTasks` is properly populated
            var groupTasks = db.Tasks.Where(t => t.TeamId == id).ToList();
            //Dec 6, 2014 10:30:00 -0800
            ViewBag.GanttData = groupTasks.Select(t => new
            {
                TaskId = t.TaskId.ToString(),
                TaskName = t.TaskName,
                StartDate = t.StartDate.ToString(@"g"),
                EndDate = t.EndDate.ToString(@"g"),
                Dependencies = t.Dependencies // Assuming Dependencies is a string that indicates task dependencies
            }).ToList();

            ViewBag.IsServiceProviderOrCoordinator = User.IsInRole("ServiceProvider") || User.IsInRole("TeamCoordinator") || User.IsInRole("TeamLeader");

            return View(team);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignRoles(TeamEmployeeViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (viewModel.Team != null && viewModel.Team.TeamMembers != null)
                {
                    // Update each team member with the assigned role
                    foreach (var member in viewModel.Team.TeamMembers)
                    {
                        var teamMember = db.TeamMembers.Find(member.TeamMemberId);
                        if (teamMember != null)
                        {
                            teamMember.Role = member.Role;
                            db.Entry(teamMember).State = EntityState.Modified;
                        }
                    }

                    db.SaveChanges();
                    return RedirectToAction("Details", new { id = viewModel.Team.TeamId });
                }
            }

            // Handle the case where TeamMembers is null or invalid state
            ModelState.AddModelError("", "No team members found or team member roles were not provided.");
            return RedirectToAction(nameof(Index));
        }

    }
}
