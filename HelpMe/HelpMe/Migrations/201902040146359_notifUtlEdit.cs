namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class notifUtlEdit : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notifications", "Url", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Notifications", "Url");
        }
    }
}
