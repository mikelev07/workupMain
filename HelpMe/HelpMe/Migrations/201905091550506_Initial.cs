namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            /*DropForeignKey("dbo.UserTaskCategories", "User_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.UserTaskCategories", "TaskCategory_Id", "dbo.TaskCategories");
            DropIndex("dbo.UserTaskCategories", new[] { "User_Id" });
            DropIndex("dbo.UserTaskCategories", new[] { "TaskCategory_Id" });
            DropTable("dbo.UserTaskCategories");*/
        }
        
        public override void Down()
        {
            /*CreateTable(
                "dbo.UserTaskCategories",
                c => new
                    {
                        User_Id = c.String(nullable: false, maxLength: 128),
                        TaskCategory_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.User_Id, t.TaskCategory_Id });
            
            CreateIndex("dbo.UserTaskCategories", "TaskCategory_Id");
            CreateIndex("dbo.UserTaskCategories", "User_Id");
            AddForeignKey("dbo.UserTaskCategories", "TaskCategory_Id", "dbo.TaskCategories", "Id", cascadeDelete: true);
            AddForeignKey("dbo.UserTaskCategories", "User_Id", "dbo.AspNetUsers", "Id", cascadeDelete: true);*/
        }
    }
}
