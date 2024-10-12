namespace EventMangementSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Announcements",
                c => new
                    {
                        AnnouncementId = c.Int(nullable: false, identity: true),
                        EventId = c.Int(nullable: false),
                        Title = c.String(),
                        Content = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.AnnouncementId)
                .ForeignKey("dbo.Events", t => t.EventId, cascadeDelete: true)
                .Index(t => t.EventId);
            
            CreateTable(
                "dbo.Events",
                c => new
                    {
                        EventId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Date = c.DateTime(nullable: false),
                        Start = c.DateTime(nullable: false),
                        End = c.DateTime(nullable: false),
                        EventMangerEmail = c.String(),
                        Location = c.String(),
                        status = c.String(),
                        Description = c.String(),
                        TicketPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Canceled = c.Boolean(nullable: false),
                        PicturePath = c.String(),
                        VenueId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.EventId)
                .ForeignKey("dbo.Venues", t => t.VenueId, cascadeDelete: true)
                .Index(t => t.VenueId);
            
            CreateTable(
                "dbo.EventInventories",
                c => new
                    {
                        EventInventoryId = c.Int(nullable: false, identity: true),
                        EventId = c.Int(nullable: false),
                        InventoryId = c.Int(nullable: false),
                        QuantityRequired = c.Int(nullable: false),
                        UniqueCode = c.Int(nullable: false),
                        DriverSignature = c.String(),
                        AdminSignature = c.String(),
                        Status = c.String(),
                        Email = c.String(),
                        QrCodePicture = c.String(),
                        Address = c.String(),
                        DriverEmail = c.String(),
                        FirstName = c.String(),
                        ManagerSignature = c.String(),
                        PreferredTime = c.String(),
                        DeliveredBy = c.Int(nullable: false),
                        DeliveryDate = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DeliveredOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        IsDeliveryRescheduled = c.Boolean(nullable: false),
                        Inventory_InventoryId = c.Int(),
                    })
                .PrimaryKey(t => t.EventInventoryId)
                .ForeignKey("dbo.Events", t => t.EventId, cascadeDelete: true)
                .ForeignKey("dbo.Inventories", t => t.Inventory_InventoryId)
                .Index(t => t.EventId)
                .Index(t => t.Inventory_InventoryId);
            
            CreateTable(
                "dbo.Inventories",
                c => new
                    {
                        InventoryId = c.Int(nullable: false, identity: true),
                        ItemName = c.String(),
                        Description = c.String(),
                        picture = c.String(),
                        QuantityAvailable = c.Int(nullable: false),
                        EventInventory_EventInventoryId = c.Int(),
                    })
                .PrimaryKey(t => t.InventoryId)
                .ForeignKey("dbo.EventInventories", t => t.EventInventory_EventInventoryId)
                .Index(t => t.EventInventory_EventInventoryId);
            
            CreateTable(
                "dbo.ServiceRequests",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EventId = c.Int(nullable: false),
                        ServiceName = c.String(),
                        ServiceCategory = c.String(nullable: false),
                        Priority = c.String(),
                        EventManagerSignature = c.String(),
                        ServiceProviderSignature = c.String(),
                        IsAssigned = c.Boolean(nullable: false),
                        ServiceProviderId = c.Int(),
                        IsOpenForBidding = c.Boolean(nullable: false),
                        IsCompleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Events", t => t.EventId, cascadeDelete: true)
                .ForeignKey("dbo.ServiceProviders", t => t.ServiceProviderId)
                .Index(t => t.EventId)
                .Index(t => t.ServiceProviderId);
            
            CreateTable(
                "dbo.Quotations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ServiceRequestId = c.Int(nullable: false),
                        ServiceProviderId = c.Int(nullable: false),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Description = c.String(),
                        IsApproved = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ServiceProviders", t => t.ServiceProviderId, cascadeDelete: true)
                .ForeignKey("dbo.ServiceRequests", t => t.ServiceRequestId, cascadeDelete: true)
                .Index(t => t.ServiceRequestId)
                .Index(t => t.ServiceProviderId);
            
            CreateTable(
                "dbo.ServiceProviders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Specialization = c.String(),
                        email = c.String(),
                        ContactInfo = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Employees",
                c => new
                    {
                        EmployeeId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Email = c.String(nullable: false),
                        Position = c.String(),
                        DateHired = c.DateTime(nullable: false),
                        ServiceProviderId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.EmployeeId)
                .ForeignKey("dbo.ServiceProviders", t => t.ServiceProviderId, cascadeDelete: true)
                .Index(t => t.ServiceProviderId);
            
            CreateTable(
                "dbo.Inventory2",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ServiceProviderId = c.Int(nullable: false),
                        EquipmentName = c.String(nullable: false),
                        QuantityAvailable = c.Int(nullable: false),
                        IsAvailable = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ServiceProviders", t => t.ServiceProviderId, cascadeDelete: true)
                .Index(t => t.ServiceProviderId);
            
            CreateTable(
                "dbo.Venues",
                c => new
                    {
                        VenueId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Address = c.String(),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.VenueId);
            
            CreateTable(
                "dbo.ChatMessages",
                c => new
                    {
                        ChatMessageId = c.Int(nullable: false, identity: true),
                        ChatSessionId = c.Int(nullable: false),
                        UserId = c.String(maxLength: 128),
                        Message = c.String(),
                        Timestamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ChatMessageId)
                .ForeignKey("dbo.ChatSessions", t => t.ChatSessionId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.ChatSessionId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.ChatSessions",
                c => new
                    {
                        ChatSessionId = c.Int(nullable: false, identity: true),
                        EventId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ChatSessionId)
                .ForeignKey("dbo.Events", t => t.EventId, cascadeDelete: true)
                .Index(t => t.EventId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.Donations",
                c => new
                    {
                        DonationId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Surname = c.String(),
                        Email = c.String(),
                        Description = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        EventId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.DonationId)
                .ForeignKey("dbo.Events", t => t.EventId, cascadeDelete: true)
                .Index(t => t.EventId);
            
            CreateTable(
                "dbo.DriverAssignments",
                c => new
                    {
                        AssDrivId = c.Int(nullable: false, identity: true),
                        EventInventoryId = c.Int(nullable: false),
                        DrivId = c.Int(nullable: false),
                        Name = c.String(),
                        Surname = c.String(),
                        Email = c.String(),
                        Status = c.String(),
                        DeliveryDate = c.String(),
                        DeliveryTime = c.String(),
                        Created = c.DateTime(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        GenDeliveryDate = c.String(),
                        PreferredTime = c.String(),
                    })
                .PrimaryKey(t => t.AssDrivId)
                .ForeignKey("dbo.Drivers", t => t.DrivId, cascadeDelete: true)
                .ForeignKey("dbo.EventInventories", t => t.EventInventoryId, cascadeDelete: true)
                .Index(t => t.EventInventoryId)
                .Index(t => t.DrivId);
            
            CreateTable(
                "dbo.Drivers",
                c => new
                    {
                        DrivId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Surname = c.String(),
                        Email = c.String(),
                        Picture = c.String(),
                        IsAvailable = c.Boolean(nullable: false),
                        CarName = c.String(),
                        CarModel = c.String(),
                        CarReg = c.String(),
                        CarType = c.String(),
                        PhoneNumber = c.String(),
                        Address = c.String(),
                    })
                .PrimaryKey(t => t.DrivId);
            
            CreateTable(
                "dbo.EventEvaluations",
                c => new
                    {
                        EventEvaluationId = c.Int(nullable: false, identity: true),
                        EventId = c.Int(nullable: false),
                        AttendeeName = c.String(),
                        Feedback = c.String(),
                        Rating = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.EventEvaluationId)
                .ForeignKey("dbo.Events", t => t.EventId, cascadeDelete: true)
                .Index(t => t.EventId);
            
            CreateTable(
                "dbo.EventReminders",
                c => new
                    {
                        EventReminderId = c.Int(nullable: false, identity: true),
                        EventId = c.Int(nullable: false),
                        UserEmail = c.String(),
                        ReminderTime = c.DateTime(nullable: false),
                        IsSent = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.EventReminderId)
                .ForeignKey("dbo.Events", t => t.EventId, cascadeDelete: true)
                .Index(t => t.EventId);
            
            CreateTable(
                "dbo.Notifications",
                c => new
                    {
                        NotificationId = c.Int(nullable: false, identity: true),
                        Message = c.String(nullable: false, maxLength: 255),
                        IsRead = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.NotificationId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.ServiceCategories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TeamMembers",
                c => new
                    {
                        TeamMemberId = c.Int(nullable: false, identity: true),
                        EmployeeId = c.Int(nullable: false),
                        TeamId = c.Int(nullable: false),
                        Role = c.String(),
                    })
                .PrimaryKey(t => t.TeamMemberId)
                .ForeignKey("dbo.Employees", t => t.EmployeeId, cascadeDelete: true)
                .ForeignKey("dbo.Teams", t => t.TeamId, cascadeDelete: true)
                .Index(t => t.EmployeeId)
                .Index(t => t.TeamId);
            
            CreateTable(
                "dbo.Teams",
                c => new
                    {
                        TeamId = c.Int(nullable: false, identity: true),
                        TeamName = c.String(nullable: false),
                        Description = c.String(),
                        ServiceProviderId = c.Int(),
                    })
                .PrimaryKey(t => t.TeamId)
                .ForeignKey("dbo.ServiceProviders", t => t.ServiceProviderId)
                .Index(t => t.ServiceProviderId);
            
            CreateTable(
                "dbo.Tickets",
                c => new
                    {
                        TicketId = c.Int(nullable: false, identity: true),
                        EventId = c.Int(nullable: false),
                        AttendeeName = c.String(),
                        AttendeeEmail = c.String(),
                        Quantity = c.Int(nullable: false),
                        tempqty = c.Int(nullable: false),
                        TotalPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CheckInCode = c.String(),
                        QRCode = c.String(),
                        ChargeID = c.String(),
                        IsCheckedIn = c.Boolean(nullable: false),
                        Refunded = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.TicketId)
                .ForeignKey("dbo.Events", t => t.EventId, cascadeDelete: true)
                .Index(t => t.EventId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Tickets", "EventId", "dbo.Events");
            DropForeignKey("dbo.TeamMembers", "TeamId", "dbo.Teams");
            DropForeignKey("dbo.Teams", "ServiceProviderId", "dbo.ServiceProviders");
            DropForeignKey("dbo.TeamMembers", "EmployeeId", "dbo.Employees");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.Notifications", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.EventReminders", "EventId", "dbo.Events");
            DropForeignKey("dbo.EventEvaluations", "EventId", "dbo.Events");
            DropForeignKey("dbo.DriverAssignments", "EventInventoryId", "dbo.EventInventories");
            DropForeignKey("dbo.DriverAssignments", "DrivId", "dbo.Drivers");
            DropForeignKey("dbo.Donations", "EventId", "dbo.Events");
            DropForeignKey("dbo.ChatMessages", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ChatSessions", "EventId", "dbo.Events");
            DropForeignKey("dbo.ChatMessages", "ChatSessionId", "dbo.ChatSessions");
            DropForeignKey("dbo.Announcements", "EventId", "dbo.Events");
            DropForeignKey("dbo.Events", "VenueId", "dbo.Venues");
            DropForeignKey("dbo.ServiceRequests", "ServiceProviderId", "dbo.ServiceProviders");
            DropForeignKey("dbo.ServiceRequests", "EventId", "dbo.Events");
            DropForeignKey("dbo.Quotations", "ServiceRequestId", "dbo.ServiceRequests");
            DropForeignKey("dbo.Quotations", "ServiceProviderId", "dbo.ServiceProviders");
            DropForeignKey("dbo.Inventory2", "ServiceProviderId", "dbo.ServiceProviders");
            DropForeignKey("dbo.Employees", "ServiceProviderId", "dbo.ServiceProviders");
            DropForeignKey("dbo.EventInventories", "Inventory_InventoryId", "dbo.Inventories");
            DropForeignKey("dbo.Inventories", "EventInventory_EventInventoryId", "dbo.EventInventories");
            DropForeignKey("dbo.EventInventories", "EventId", "dbo.Events");
            DropIndex("dbo.Tickets", new[] { "EventId" });
            DropIndex("dbo.Teams", new[] { "ServiceProviderId" });
            DropIndex("dbo.TeamMembers", new[] { "TeamId" });
            DropIndex("dbo.TeamMembers", new[] { "EmployeeId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.Notifications", new[] { "UserId" });
            DropIndex("dbo.EventReminders", new[] { "EventId" });
            DropIndex("dbo.EventEvaluations", new[] { "EventId" });
            DropIndex("dbo.DriverAssignments", new[] { "DrivId" });
            DropIndex("dbo.DriverAssignments", new[] { "EventInventoryId" });
            DropIndex("dbo.Donations", new[] { "EventId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.ChatSessions", new[] { "EventId" });
            DropIndex("dbo.ChatMessages", new[] { "UserId" });
            DropIndex("dbo.ChatMessages", new[] { "ChatSessionId" });
            DropIndex("dbo.Inventory2", new[] { "ServiceProviderId" });
            DropIndex("dbo.Employees", new[] { "ServiceProviderId" });
            DropIndex("dbo.Quotations", new[] { "ServiceProviderId" });
            DropIndex("dbo.Quotations", new[] { "ServiceRequestId" });
            DropIndex("dbo.ServiceRequests", new[] { "ServiceProviderId" });
            DropIndex("dbo.ServiceRequests", new[] { "EventId" });
            DropIndex("dbo.Inventories", new[] { "EventInventory_EventInventoryId" });
            DropIndex("dbo.EventInventories", new[] { "Inventory_InventoryId" });
            DropIndex("dbo.EventInventories", new[] { "EventId" });
            DropIndex("dbo.Events", new[] { "VenueId" });
            DropIndex("dbo.Announcements", new[] { "EventId" });
            DropTable("dbo.Tickets");
            DropTable("dbo.Teams");
            DropTable("dbo.TeamMembers");
            DropTable("dbo.ServiceCategories");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.Notifications");
            DropTable("dbo.EventReminders");
            DropTable("dbo.EventEvaluations");
            DropTable("dbo.Drivers");
            DropTable("dbo.DriverAssignments");
            DropTable("dbo.Donations");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.ChatSessions");
            DropTable("dbo.ChatMessages");
            DropTable("dbo.Venues");
            DropTable("dbo.Inventory2");
            DropTable("dbo.Employees");
            DropTable("dbo.ServiceProviders");
            DropTable("dbo.Quotations");
            DropTable("dbo.ServiceRequests");
            DropTable("dbo.Inventories");
            DropTable("dbo.EventInventories");
            DropTable("dbo.Events");
            DropTable("dbo.Announcements");
        }
    }
}
