namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserNotEdit9a1a : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notifications", "ExUserName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Notifications", "ExUserName");
        }
    }
}
