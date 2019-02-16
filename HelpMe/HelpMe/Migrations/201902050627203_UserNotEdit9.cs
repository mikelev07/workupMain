namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserNotEdit9 : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Notifications", name: "UserId", newName: "User_Id");
            RenameIndex(table: "dbo.Notifications", name: "IX_UserId", newName: "IX_User_Id");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.Notifications", name: "IX_User_Id", newName: "IX_UserId");
            RenameColumn(table: "dbo.Notifications", name: "User_Id", newName: "UserId");
        }
    }
}
