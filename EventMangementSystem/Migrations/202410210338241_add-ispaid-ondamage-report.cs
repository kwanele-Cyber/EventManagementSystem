namespace EventMangementSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addispaidondamagereport : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DamageReports", "IsPaid", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DamageReports", "IsPaid");
        }
    }
}
