namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class andnewfix11211 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Notifications", "CustomId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Notifications", "CustomId", c => c.String());
        }
    }
}
