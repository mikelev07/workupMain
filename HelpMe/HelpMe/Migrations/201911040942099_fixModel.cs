namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notifications", "Description", c => c.String());
            DropColumn("dbo.Notifications", "DescriptionFrom");
            DropColumn("dbo.Notifications", "DescriptionTo");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Notifications", "DescriptionTo", c => c.String());
            AddColumn("dbo.Notifications", "DescriptionFrom", c => c.String());
            DropColumn("dbo.Notifications", "Description");
        }
    }
}
