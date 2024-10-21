namespace EventMangementSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addhasAccepted : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TeamMembers", "HasAccepted", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TeamMembers", "HasAccepted");
        }
    }
}
