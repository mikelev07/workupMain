namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReviewCustomChanges : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomViewModels", "DoneInTime", c => c.Boolean());
            AddColumn("dbo.Reviews", "Rating", c => c.Int(nullable: false));
            AddColumn("dbo.Reviews", "Date", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Reviews", "Date");
            DropColumn("dbo.Reviews", "Rating");
            DropColumn("dbo.CustomViewModels", "DoneInTime");
        }
    }
}
