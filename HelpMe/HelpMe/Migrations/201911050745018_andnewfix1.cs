namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class andnewfix1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notifications", "ExecutorName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Notifications", "ExecutorName");
        }
    }
}
