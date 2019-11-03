namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class andNeeeew1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notifications", "DescriptionFrom", c => c.String());
            AddColumn("dbo.Notifications", "DescriptionTo", c => c.String());
            AddColumn("dbo.Notifications", "StartDate", c => c.DateTime(nullable: false));
            DropColumn("dbo.Notifications", "Description");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Notifications", "Description", c => c.String());
            DropColumn("dbo.Notifications", "StartDate");
            DropColumn("dbo.Notifications", "DescriptionTo");
            DropColumn("dbo.Notifications", "DescriptionFrom");
        }
    }
}
