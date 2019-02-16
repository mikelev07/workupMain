namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class notifUserNameEdit : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notifications", "UserName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Notifications", "UserName");
        }
    }
}
