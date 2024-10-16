namespace EventMangementSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateD : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ServiceRequests", "IsTeamAssigned", c => c.Boolean(nullable: false));
            AddColumn("dbo.GroupTasks", "ServiceRequest_Id", c => c.Int());
            CreateIndex("dbo.GroupTasks", "ServiceRequest_Id");
            AddForeignKey("dbo.GroupTasks", "ServiceRequest_Id", "dbo.ServiceRequests", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GroupTasks", "ServiceRequest_Id", "dbo.ServiceRequests");
            DropIndex("dbo.GroupTasks", new[] { "ServiceRequest_Id" });
            DropColumn("dbo.GroupTasks", "ServiceRequest_Id");
            DropColumn("dbo.ServiceRequests", "IsTeamAssigned");
        }
    }
}
