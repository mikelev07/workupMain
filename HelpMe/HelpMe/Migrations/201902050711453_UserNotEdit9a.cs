namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserNotEdit9a : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Notifications", name: "User_Id", newName: "UserId");
            RenameIndex(table: "dbo.Notifications", name: "IX_User_Id", newName: "IX_UserId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.Notifications", name: "IX_UserId", newName: "IX_User_Id");
            RenameColumn(table: "dbo.Notifications", name: "UserId", newName: "User_Id");
        }
    }
}
