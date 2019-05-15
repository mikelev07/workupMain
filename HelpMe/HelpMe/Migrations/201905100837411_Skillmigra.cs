namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Skillmigra : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Skills",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        TaskCategoryId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TaskCategories", t => t.TaskCategoryId)
                .Index(t => t.TaskCategoryId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Skills", "TaskCategoryId", "dbo.TaskCategories");
            DropIndex("dbo.Skills", new[] { "TaskCategoryId" });
            DropTable("dbo.Skills");
        }
    }
}
