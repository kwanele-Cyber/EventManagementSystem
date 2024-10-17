namespace EventMangementSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddActualEndTimeinGroupTasks : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GroupTasks", "ActualEndTime", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.GroupTasks", "ActualEndTime");
        }
    }
}
