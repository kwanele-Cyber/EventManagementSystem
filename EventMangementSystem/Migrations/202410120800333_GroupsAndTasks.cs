namespace EventMangementSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GroupsAndTasks : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GroupTasks",
                c => new
                    {
                        TaskId = c.Int(nullable: false, identity: true),
                        TaskName = c.String(nullable: false),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        EmployeeId = c.Int(nullable: false),
                        TeamId = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        Dependencies = c.String(),
                        Progress = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.TaskId)
                .ForeignKey("dbo.Employees", t => t.EmployeeId, cascadeDelete: true)
                .ForeignKey("dbo.Teams", t => t.TeamId, cascadeDelete: true)
                .Index(t => t.EmployeeId)
                .Index(t => t.TeamId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GroupTasks", "TeamId", "dbo.Teams");
            DropForeignKey("dbo.GroupTasks", "EmployeeId", "dbo.Employees");
            DropIndex("dbo.GroupTasks", new[] { "TeamId" });
            DropIndex("dbo.GroupTasks", new[] { "EmployeeId" });
            DropTable("dbo.GroupTasks");
        }
    }
}
