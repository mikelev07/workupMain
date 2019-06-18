namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserViewModelRevCust : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomViewModels", "UserViewModel_Id", c => c.String(maxLength: 128));
            AddColumn("dbo.Reviews", "UserViewModel_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.CustomViewModels", "UserViewModel_Id");
            CreateIndex("dbo.Reviews", "UserViewModel_Id");
            AddForeignKey("dbo.CustomViewModels", "UserViewModel_Id", "dbo.UserViewModels", "Id");
            AddForeignKey("dbo.Reviews", "UserViewModel_Id", "dbo.UserViewModels", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Reviews", "UserViewModel_Id", "dbo.UserViewModels");
            DropForeignKey("dbo.CustomViewModels", "UserViewModel_Id", "dbo.UserViewModels");
            DropIndex("dbo.Reviews", new[] { "UserViewModel_Id" });
            DropIndex("dbo.CustomViewModels", new[] { "UserViewModel_Id" });
            DropColumn("dbo.Reviews", "UserViewModel_Id");
            DropColumn("dbo.CustomViewModels", "UserViewModel_Id");
        }
    }
}
