namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class andnewfix : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notifications", "CustomName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Notifications", "CustomName");
        }
    }
}
