namespace EventMangementSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StartCodeIntoServiceRequest : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ServiceRequests", "Status", c => c.Int(nullable: false));
            AddColumn("dbo.ServiceRequests", "StartCode", c => c.String());
            AddColumn("dbo.ServiceRequests", "FinishCode", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ServiceRequests", "FinishCode");
            DropColumn("dbo.ServiceRequests", "StartCode");
            DropColumn("dbo.ServiceRequests", "Status");
        }
    }
}
