namespace EventMangementSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeInventorycreation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Inventories", "PriceToRelace", c => c.Int(nullable: false));
            AddColumn("dbo.Inventories", "PriceToService", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Inventories", "PriceToService");
            DropColumn("dbo.Inventories", "PriceToRelace");
        }
    }
}
