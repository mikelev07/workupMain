namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserNotEdit : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notifications", "UserId", c => c.String(maxLength: 128));
            CreateIndex("dbo.Notifications", "UserId");
            AddForeignKey("dbo.Notifications", "UserId", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Notifications", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.Notifications", new[] { "UserId" });
            DropColumn("dbo.Notifications", "UserId");
        }
    }
}
