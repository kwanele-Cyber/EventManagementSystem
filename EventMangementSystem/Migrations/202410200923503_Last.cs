namespace EventMangementSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Last : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.DamageReports", "TotalCost", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.DamageReports", "TotalCost", c => c.Int(nullable: false));
        }
    }
}
