namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class andnewfix11 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notifications", "CustomId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Notifications", "CustomId");
        }
    }
}
