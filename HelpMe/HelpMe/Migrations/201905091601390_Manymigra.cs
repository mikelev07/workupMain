namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Manymigra : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UseCateg",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        TaskCategoryId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.TaskCategoryId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.TaskCategories", t => t.TaskCategoryId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.TaskCategoryId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UseCateg", "TaskCategoryId", "dbo.TaskCategories");
            DropForeignKey("dbo.UseCateg", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.UseCateg", new[] { "TaskCategoryId" });
            DropIndex("dbo.UseCateg", new[] { "UserId" });
            DropTable("dbo.UseCateg");
        }
    }
}
