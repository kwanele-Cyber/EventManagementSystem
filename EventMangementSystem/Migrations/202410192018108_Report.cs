namespace EventMangementSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Report : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DamageReports",
                c => new
                    {
                        ReportId = c.Int(nullable: false, identity: true),
                        EquipmentId = c.Int(nullable: false),
                        DamageDescription = c.String(nullable: false, maxLength: 500),
                        ReportDate = c.DateTime(nullable: false),
                        EventId = c.Int(nullable: false),
                        TotalCost = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ReportId)
                .ForeignKey("dbo.Events", t => t.EventId, cascadeDelete: true)
                .ForeignKey("dbo.Inventories", t => t.EquipmentId, cascadeDelete: true)
                .Index(t => t.EquipmentId)
                .Index(t => t.EventId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DamageReports", "EquipmentId", "dbo.Inventories");
            DropForeignKey("dbo.DamageReports", "EventId", "dbo.Events");
            DropIndex("dbo.DamageReports", new[] { "EventId" });
            DropIndex("dbo.DamageReports", new[] { "EquipmentId" });
            DropTable("dbo.DamageReports");
        }
    }
}
