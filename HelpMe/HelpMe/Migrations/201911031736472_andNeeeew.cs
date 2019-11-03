namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class andNeeeew : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notifications", "UserToId", c => c.String());
            AddColumn("dbo.Notifications", "UserFromId", c => c.String());
            DropColumn("dbo.Notifications", "Title");
            DropColumn("dbo.Notifications", "Url");
            DropColumn("dbo.Notifications", "UserName");
            DropColumn("dbo.Notifications", "ExUserName");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Notifications", "ExUserName", c => c.String());
            AddColumn("dbo.Notifications", "UserName", c => c.String());
            AddColumn("dbo.Notifications", "Url", c => c.String());
            AddColumn("dbo.Notifications", "Title", c => c.String());
            DropColumn("dbo.Notifications", "UserFromId");
            DropColumn("dbo.Notifications", "UserToId");
        }
    }
}
