namespace EventMangementSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ReturnProcesses",
                c => new
                    {
                        ReturnProcessId = c.Int(nullable: false, identity: true),
                        DriverAssignmentId = c.Int(nullable: false),
                        EventInventoryId = c.Int(nullable: false),
                        QuantityReturned = c.Int(nullable: false),
                        Status = c.String(),
                        ReturnSubmittedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        InspectionCondition = c.String(),
                        InspectionNotes = c.String(),
                        RepairCost = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MissingItemCost = c.Decimal(nullable: false, precision: 18, scale: 2),
                        InspectionCompletedOn = c.DateTime(precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.ReturnProcessId)
                .ForeignKey("dbo.DriverAssignments", t => t.DriverAssignmentId)
                .ForeignKey("dbo.EventInventories", t => t.EventInventoryId)
                .Index(t => t.DriverAssignmentId)
                .Index(t => t.EventInventoryId);
            
            AddColumn("dbo.GroupTasks", "ActualEndTime", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ReturnProcesses", "EventInventoryId", "dbo.EventInventories");
            DropForeignKey("dbo.ReturnProcesses", "DriverAssignmentId", "dbo.DriverAssignments");
            DropIndex("dbo.ReturnProcesses", new[] { "EventInventoryId" });
            DropIndex("dbo.ReturnProcesses", new[] { "DriverAssignmentId" });
            DropColumn("dbo.GroupTasks", "ActualEndTime");
            DropTable("dbo.ReturnProcesses");
        }
    }
}
