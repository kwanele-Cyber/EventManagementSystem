using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using EventMangementSystem.Models;
using Microsoft.AspNet.Identity.EntityFramework;

namespace EventMangementSystem
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit https://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context, user manager and signin manager to use a single instance per request
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });            
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //   consumerKey: "",
            //   consumerSecret: "");

            //app.UseFacebookAuthentication(
            //   appId: "",
            //   appSecret: "");

            //app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            //{
            //    ClientId = "",
            //    ClientSecret = ""
            //});
            CreateRolesAndUsers();
        }
        private void CreateRolesAndUsers()
            {
                ApplicationDbContext db = new ApplicationDbContext();
            
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            
                // Admin role and user
                if (!roleManager.RoleExists("Admin"))
                {
                    roleManager.Create(new IdentityRole("Admin"));
            
                    var user = new ApplicationUser();
                    user.Name = "Admin";
                    user.UserName = "Admin@event.com";
                    user.Email = "Admin@event.com";
                    user.EmailConfirmed = true;
                    string pwd = "Event@01";
            
                    var newuser = userManager.Create(user, pwd);
                    if (newuser.Succeeded)
                    {
                        userManager.AddToRole(user.Id, "Admin");
                    }
                }
            
                // ServiceProvider role and user
                if (!roleManager.RoleExists("ServiceProvider"))
                {
                    roleManager.Create(new IdentityRole("ServiceProvider"));
            
                    var serviceProviderUser = new ApplicationUser();
                    serviceProviderUser.Name = "Catering Co.";
                    serviceProviderUser.UserName = "serviceprovider@event.com";
                    serviceProviderUser.Email = "serviceprovider@event.com";
                    serviceProviderUser.EmailConfirmed = true;
                    string serviceProviderPwd = "Service@01";
            
                    var newServiceProviderUser = userManager.Create(serviceProviderUser, serviceProviderPwd);
                    if (newServiceProviderUser.Succeeded)
                    {
                        userManager.AddToRole(serviceProviderUser.Id, "ServiceProvider");
            
                        // Create corresponding ServiceProvider entity
                        var serviceProvider = new ServiceProvider
                        {
                            Name = "Catering Co.",
                            Specialization = "Catering",
                            email = "serviceprovider@event.com",
                            ContactInfo = "0857208560",
                        };
                        db.ServiceProviders.Add(serviceProvider);
                    }
                }
            
                // Employee role and user
                if (!roleManager.RoleExists("Employee"))
                {
                    roleManager.Create(new IdentityRole("Employee"));
            
                    var employeeUser = new ApplicationUser();
                    employeeUser.Name = "John Employee";
                    employeeUser.UserName = "employee@event.com";
                    employeeUser.Email = "employee@event.com";
                    employeeUser.EmailConfirmed = true;
                    string employeePwd = "Employee@01";
            
                    var newEmployeeUser = userManager.Create(employeeUser, employeePwd);
                    if (newEmployeeUser.Succeeded)
                    {
                        userManager.AddToRole(employeeUser.Id, "Employee");
            
                        // Create corresponding Employee entity
                        var employee = new Employee
                        {
                            Name = "John Employee",
                            Email = "employee@event.com",
                            Position = "Technician",
                            DateHired = DateTime.Now,
                            ServiceProviderId = db.ServiceProviders.First().Id // assuming first ServiceProvider is assigned
                        };
                        db.Employees.Add(employee);
                    }
                }
            
                // Driver role and user
                if (!roleManager.RoleExists("Driver"))
                {
                    roleManager.Create(new IdentityRole("Driver"));
            
                    var driverUser = new ApplicationUser();
                    driverUser.Name = "Jane Driver";
                    driverUser.UserName = "driver@event.com";
                    driverUser.Email = "driver@event.com";
                    driverUser.EmailConfirmed = true;
                    string driverPwd = "Driver@01";
            
                    var newDriverUser = userManager.Create(driverUser, driverPwd);
                    if (newDriverUser.Succeeded)
                    {
                        userManager.AddToRole(driverUser.Id, "Driver");
            
                        // Create corresponding Driver entity
                        var driver = new Driver
                        {
                            Name = "Jane",
                            Surname = "Driver",
                            Email = "driver@event.com",
                            IsAvailable = true,
                            CarName = "Toyota",
                            CarModel = "Corolla",
                            CarReg = "XYZ-123",
                            CarType = "Truck",
                            PhoneNumber = "0829781662",
                            Address = "1940 Main Rd, Stanger, KwaZulu-Natal, 4450, South Africa",
                        };
                        db.Drivers.Add(driver);
                    }
                }
            
                // EventManager role and user
                if (!roleManager.RoleExists("EventManager"))
                {
                    roleManager.Create(new IdentityRole("EventManager"));
            
                    var eventManagerUser = new ApplicationUser();
                    eventManagerUser.Name = "Event Manager";
                    eventManagerUser.UserName = "eventmanager@event.com";
                    eventManagerUser.Email = "eventmanager@event.com";
                    eventManagerUser.EmailConfirmed = true;
                    string eventManagerPwd = "Manager@01";
            
                    var newEventManagerUser = userManager.Create(eventManagerUser, eventManagerPwd);
                    if (newEventManagerUser.Succeeded)
                    {
                        userManager.AddToRole(eventManagerUser.Id, "EventManager");
            
                        // Optional: create an EventManager entity if needed
                    }
                }
            
                // Save all changes to the database
                db.SaveChanges();
            }


        }

    }
}
