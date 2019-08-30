namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CustomNullableDateMigration : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomViewModels", "PlagiarismPercentage", c => c.Int());
            AlterColumn("dbo.CustomViewModels", "EndingDate", c => c.DateTime());
            AlterColumn("dbo.CustomViewModels", "ExecutorStartDate", c => c.DateTime());
            AlterColumn("dbo.CustomViewModels", "Price", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.CustomViewModels", "Price", c => c.Int(nullable: false));
            AlterColumn("dbo.CustomViewModels", "ExecutorStartDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.CustomViewModels", "EndingDate", c => c.DateTime(nullable: false));
            DropColumn("dbo.CustomViewModels", "PlagiarismPercentage");
        }
    }
}
