namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class nothing : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserViewModels",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Username = c.String(),
                        ImagePath = c.String(),
                        Email = c.String(),
                        Role = c.String(),
                        PositiveThumbs = c.Int(nullable: false),
                        NegativeThumbs = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.TaskCategories", "UserViewModel_Id", c => c.String(maxLength: 128));
            AddColumn("dbo.Skills", "UserViewModel_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.TaskCategories", "UserViewModel_Id");
            CreateIndex("dbo.Skills", "UserViewModel_Id");
            AddForeignKey("dbo.Skills", "UserViewModel_Id", "dbo.UserViewModels", "Id");
            AddForeignKey("dbo.TaskCategories", "UserViewModel_Id", "dbo.UserViewModels", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TaskCategories", "UserViewModel_Id", "dbo.UserViewModels");
            DropForeignKey("dbo.Skills", "UserViewModel_Id", "dbo.UserViewModels");
            DropIndex("dbo.Skills", new[] { "UserViewModel_Id" });
            DropIndex("dbo.TaskCategories", new[] { "UserViewModel_Id" });
            DropColumn("dbo.Skills", "UserViewModel_Id");
            DropColumn("dbo.TaskCategories", "UserViewModel_Id");
            DropTable("dbo.UserViewModels");
        }
    }
}
