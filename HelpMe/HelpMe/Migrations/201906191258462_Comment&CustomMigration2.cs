namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CommentCustomMigration2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomViewModels", "ExecutorStartDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.CommentViewModels", "Hours", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CommentViewModels", "Hours");
            DropColumn("dbo.CustomViewModels", "ExecutorStartDate");
        }
    }
}
