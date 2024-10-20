namespace EventMangementSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FindRecord : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.DamageReports", "findRecord");
            AddForeignKey("dbo.DamageReports", "findRecord", "dbo.ReturnProcesses", "ReturnProcessId", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DamageReports", "findRecord", "dbo.ReturnProcesses");
            DropIndex("dbo.DamageReports", new[] { "findRecord" });
        }
    }
}
