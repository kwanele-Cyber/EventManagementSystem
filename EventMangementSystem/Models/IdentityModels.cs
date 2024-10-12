using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Sockets;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PayPal.Api;

namespace EventMangementSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DbSet<Event> Events { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Venue> Venues { get; set; }
        public DbSet<EventInventory> EventInventories { get; set; }
        public DbSet<EventEvaluation> EventEvaluations { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<EventReminder> EventReminders { get; set; }
        public DbSet<Donation> Donations { get; set; }
        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<DriverAssignment> DriverAssignments { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<ServiceProvider> ServiceProviders { get; set; }
        public DbSet<Quotation> Quotations { get; set; }
        public DbSet<Inventory2> Inventories2 { get; set; }
        public DbSet<ServiceCategory> ServiceCategories { get; set; }

        //new Entities
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }

        public DbSet<Task> Tasks { get; set; }
    }
}


