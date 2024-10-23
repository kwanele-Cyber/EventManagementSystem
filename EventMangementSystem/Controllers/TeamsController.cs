using EventMangementSystem.Models;
using EventMangementSystem.ViewModels;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Diagnostics;
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
            // Reload the employees list in case of an error
            var currentUserEmail = User.Identity.GetUserName();
            var serviceProvider = db.ServiceProviders.FirstOrDefault(sp => sp.email == currentUserEmail);

            //set service provider for service provider if not set.
            if (viewModel.Team.ServiceProvider == null|| viewModel.Team.ServiceProviderId == null)
            {
                viewModel.Team.ServiceProviderId = serviceProvider.Id;
                viewModel.Team.ServiceProvider = serviceProvider;
            }

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
            //reload employees
            viewModel.Employees = db.Employees
                .Where(e => e.ServiceProviderId == serviceProvider.Id)
                .ToList();

            return View(viewModel);
        }

        [Authorize]
        public ActionResult Accept(int memberId, int id)
        {
            var teamMember = db.TeamMembers.Where(t => t.TeamMemberId == memberId).FirstOrDefault();
            teamMember.HasAccepted = true;

            db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }


        // GET: Team/Index
        [Authorize]
        public ActionResult Index()
        {
            var email = User.Identity.GetUserName();
            ViewBag.Email = email;

            if(User.IsInRole("ServiceProvider"))
            {
                var serviceProvider = db.ServiceProviders.FirstOrDefault(sp => sp.email == email);
                if (serviceProvider != null)
                    ViewBag.ServiceProviderId = serviceProvider.Id;
            }


            List<Team> teams = new List<Team>();


            teams = db.Teams
                .Include(t => t.GroupTasks)
                .Include(t => t.TeamMembers.Select(tm => tm.Employee))
                .Include(t => t.ServiceProvider)
                .Include(t => t.ServiceRequests)
                .ToList();

            return View(teams);
        }

        // GET: Team/Edit/5

        [Authorize]
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
        [Authorize]
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
        [Authorize]
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

        [Authorize]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Include related tasks and team members
            Team team = db.Teams
                .Include(t => t.GroupTasks.Select(gt => gt.Employee)) // Ensure tasks include employees
                .Include(t => t.TeamMembers.Select(tm => tm.Employee)) // Ensure team members include employees
                .FirstOrDefault(t => t.TeamId == id);


            if (team == null)
            {
                return HttpNotFound();
            }

            ViewData["GanttData"] = new List<EventMangementSystem.Models.GroupTaskViewModel>();
            // Assuming `GroupTasks` is properly populated
            ViewData["GanttData"] = db.Tasks.Include(t => t.Employee).Where(t => t.TeamId == id).Select(t => new GroupTaskViewModel
            {
                TaskId = t.TaskId.ToString(),
                TaskName = t.TaskName,
                StartDate = new TaskTime
                {
                    Hour = t.StartDate.Hour,
                    Minute = t.StartDate.Minute,
                    Second = t.StartDate.Second,
                    Day = t.StartDate.Day,
                    Month = t.StartDate.Month - 1, // JavaScript Date months are 0-based
                    Year = t.StartDate.Year
                },
                EndDate = new TaskTime
                {
                    Hour = t.EndDate.Hour,
                    Minute = t.EndDate.Minute,
                    Second = t.EndDate.Second,
                    Day = t.EndDate.Day,
                    Month = t.EndDate.Month - 1, // JavaScript Date months are 0-based
                    Year = t.EndDate.Year
                },
                Dependencies = t.Dependencies, // Assuming Dependencies is a string that indicates task dependencies
                Progress = t.Progress,
                AssignedTo = t.Employee
            }).ToList();
            //Dec 6, 2014 10:30:00 -0800


            // Check user roles for service provider, team coordinator, or leader
            var role = User.IsInRole("ServiceProvider") ? "ServiceProvider" :
                       User.IsInRole("Driver") ? "Driver" :
                       User.IsInRole("User") ? "User" :
                       User.IsInRole("Employee") ? "Employee" :
                       User.IsInRole("EventOrganiser") ? "EventOrganiser" :
                       User.IsInRole("Admin") ? "Admin" : null;

            ViewBag.Role = role;

            ViewBag.IsLeader = (role == "TeamLeader") ? true : false;

            return View(team);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
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



        [HttpGet]
        [Authorize(Roles = "ServiceProvider")]
        public ActionResult AssignServiceRequest(int teamId, string returnUrl)
        {
            // Reload the employees list in case of an error
            var currentUserEmail = User.Identity.GetUserName();
            var serviceProvider = db.ServiceProviders.FirstOrDefault(sp => sp.email == currentUserEmail);

            // Fetch the team details using the teamId
            var team = db.Teams.Include(t => t.ServiceRequests).FirstOrDefault(t => t.TeamId == teamId);

            if (team == null)
            {
                return HttpNotFound();
            }

            // Fetch available service requests that are not assigned to any team
            var availableServiceRequests = db.ServiceRequests
                .Include(t => t.Event)
                .Include(t => t.ServiceProvider)
                .Where(t => (t.TeamId != teamId || t.Status == ServiceRequestStatus.Assigned) && t.ServiceProviderId == serviceProvider.Id)
                .ToList();

            // Create a ViewModel to pass to the view
            var model = new AssignServiceRequestViewModel
            {
                TeamId = teamId,
                TeamName = team.TeamName,
                AvailableServiceRequests = availableServiceRequests,
                ReturnUrl = returnUrl
            };

            return View(model); // Pass the ViewModel to the view
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult AssignServiceRequest(int teamId, string returnUrl, int serviceRequestId)
        {
            var team = db.Teams.Include(t => t.ServiceRequests).FirstOrDefault(t => t.TeamId == teamId);
            var serviceRequest = db.ServiceRequests.Include(sr => sr.Event).FirstOrDefault(sr => sr.Id == serviceRequestId);

            if (team == null || serviceRequest == null)
            {
                return HttpNotFound();
            }

            // Check for time conflicts with existing service requests assigned to the team
            bool hasTimeConflict = team.ServiceRequests.Any(sr =>
                (serviceRequest.Event.Start < sr.Event.End && serviceRequest.Event.End > sr.Event.Start)
            );



            if (hasTimeConflict)
            {
                TempData["ErrorMessage"] = "Time conflict: The team is already assigned to another service request during this period.";
                return RedirectToAction("AssignServiceRequest", new { teamId = teamId, returnUrl = returnUrl });
            }


            serviceRequest.TeamId = teamId;
            db.Entry(serviceRequest).State = EntityState.Modified;

            // Assign the service request to the team
            team.ServiceRequests.Add(serviceRequest);
            db.Entry(team).State = EntityState.Modified;

            db.SaveChanges();

            TempData["SuccessMessage"] = $"Service request has been successfully assigned to team: {team.TeamName}.";

            // Redirect to returnUrl or fallback to a default action
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Teams");
        }


    }
}
