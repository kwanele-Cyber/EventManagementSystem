namespace EventMangementSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class inspection : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.InspectionDetails",
                c => new
                    {
                        InspectionDetailsId = c.Int(nullable: false, identity: true),
                        Condition = c.String(),
                        Notes = c.String(),
                        RepairCost = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MissingItemCost = c.Decimal(nullable: false, precision: 18, scale: 2),
                        InspectionCompletedOn = c.DateTime(precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.InspectionDetailsId);
            
            AddColumn("dbo.ReturnProcesses", "InspectionDetails_InspectionDetailsId", c => c.Int());
            CreateIndex("dbo.ReturnProcesses", "InspectionDetails_InspectionDetailsId");
            AddForeignKey("dbo.ReturnProcesses", "InspectionDetails_InspectionDetailsId", "dbo.InspectionDetails", "InspectionDetailsId");
            DropColumn("dbo.ReturnProcesses", "InspectionCondition");
            DropColumn("dbo.ReturnProcesses", "InspectionNotes");
            DropColumn("dbo.ReturnProcesses", "RepairCost");
            DropColumn("dbo.ReturnProcesses", "MissingItemCost");
            DropColumn("dbo.ReturnProcesses", "InspectionCompletedOn");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ReturnProcesses", "InspectionCompletedOn", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AddColumn("dbo.ReturnProcesses", "MissingItemCost", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.ReturnProcesses", "RepairCost", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.ReturnProcesses", "InspectionNotes", c => c.String());
            AddColumn("dbo.ReturnProcesses", "InspectionCondition", c => c.String());
            DropForeignKey("dbo.ReturnProcesses", "InspectionDetails_InspectionDetailsId", "dbo.InspectionDetails");
            DropIndex("dbo.ReturnProcesses", new[] { "InspectionDetails_InspectionDetailsId" });
            DropColumn("dbo.ReturnProcesses", "InspectionDetails_InspectionDetailsId");
            DropTable("dbo.InspectionDetails");
        }
    }
}
