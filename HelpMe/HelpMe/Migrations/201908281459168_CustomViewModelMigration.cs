namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CustomViewModelMigration : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomViewModels", "TimeBlock", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CustomViewModels", "TimeBlock");
        }
    }
}
