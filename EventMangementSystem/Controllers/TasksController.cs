using System;
using System.Linq;
using System.Web.Mvc;
using EventMangementSystem.Models;
using EventMangementSystem.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;

namespace EventMangementSystem.Controllers
{

    public class TasksController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Tasks/Create
        public ActionResult Create(int teamId)
        {
            var team = db.Teams.Include(t => t.TeamMembers.Select(tm => tm.Employee)).FirstOrDefault(t => t.TeamId == teamId);
            if (team == null)
            {
                return HttpNotFound("Team not found.");
            }

            var model = new TaskViewModel
            {
                TeamId = team.TeamId,
                TeamMembers = team.TeamMembers.Select(tm => tm.Employee).ToList()
            };

            return View(model);
        }

        // POST: Tasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TaskViewModel model)
        {
            if (ModelState.IsValid)
            {
                var task = new Task
                {
                    TaskName = model.TaskName,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    EmployeeId = model.EmployeeId,
                    TeamId = model.TeamId,
                    Status = TaskStatus.NotStarted
                };

                db.Tasks.Add(task);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Task created successfully!";
                return RedirectToAction("Details", "Teams", new { id = model.TeamId });
            }

            // Reload team members for the selected team
            var team = db.Teams.Include(t => t.TeamMembers.Select(tm => tm.Employee)).FirstOrDefault(t => t.TeamId == model.TeamId);
            if (team != null)
            {
                model.TeamMembers = team.TeamMembers.Select(tm => tm.Employee).ToList();
            }

            return View(model);
        }

        // GET: Tasks/Index
        public ActionResult Index(int teamId)
        {
            var tasks = db.Tasks.Where(t => t.TeamId == teamId).Include(t => t.Employee).ToList();
            ViewBag.TeamId = teamId;
            return View(tasks);
        }

        // GET: Tasks/AssignRoles
        public ActionResult AssignRoles(int teamId)
        {
            var team = db.Teams.Include(t => t.TeamMembers.Select(tm => tm.Employee)).FirstOrDefault(t => t.TeamId == teamId);
            if (team == null)
            {
                return HttpNotFound("Team not found.");
            }

            var model = new TeamEmployeeViewModel
            {
                Team = team,
                Employees = team.TeamMembers.Select(tm => tm.Employee).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignRoles(TeamEmployeeViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                foreach (var member in viewModel.Team.TeamMembers)
                {
                    var teamMember = db.TeamMembers.Find(member.TeamMemberId);
                    if (teamMember != null)
                    {
                        teamMember.Role = member.Role;
                        db.Entry(teamMember).State = System.Data.Entity.EntityState.Modified;
                    }
                }

                db.SaveChanges();
                return RedirectToAction("Details", "Teams", new { id = viewModel.Team.TeamId });
            }

            return View(viewModel);
        }

        // GET: Tasks/GetGanttData
        public JsonResult GetGanttData(int teamId)
        {
            var tasks = db.Tasks.Where(t => t.TeamId == teamId).ToList();

            var ganttData = tasks.Select(t => new
            {
                TaskId = "Task_" + t.TaskId,
                TaskName = t.TaskName,
                StartDate = t.StartDate.ToString("yyyy-MM-dd"),
                EndDate = t.EndDate.ToString("yyyy-MM-dd"),
                Duration = (t.EndDate - t.StartDate).Days,
                Dependencies = t.Dependencies ?? "",
                Progress = t.Progress * 100
            }).ToList();

            return Json(ganttData, JsonRequestBehavior.AllowGet);
        }
    }

}
