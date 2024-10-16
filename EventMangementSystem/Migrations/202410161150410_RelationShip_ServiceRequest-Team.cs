namespace EventMangementSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RelationShip_ServiceRequestTeam : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ServiceRequests", "TeamId", c => c.Int());
            CreateIndex("dbo.ServiceRequests", "TeamId");
            AddForeignKey("dbo.ServiceRequests", "TeamId", "dbo.Teams", "TeamId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ServiceRequests", "TeamId", "dbo.Teams");
            DropIndex("dbo.ServiceRequests", new[] { "TeamId" });
            DropColumn("dbo.ServiceRequests", "TeamId");
        }
    }
}
