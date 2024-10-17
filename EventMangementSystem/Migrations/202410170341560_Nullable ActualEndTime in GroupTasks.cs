namespace EventMangementSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NullableActualEndTimeinGroupTasks : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.GroupTasks", "ActualEndTime", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.GroupTasks", "ActualEndTime", c => c.DateTime(nullable: false));
        }
    }
}
