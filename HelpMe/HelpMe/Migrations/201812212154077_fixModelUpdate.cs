namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixModelUpdate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AttachModels", "AttachStatus", c => c.Int(nullable: false));
            DropColumn("dbo.CustomViewModels", "AttachStatus");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CustomViewModels", "AttachStatus", c => c.Int(nullable: false));
            DropColumn("dbo.AttachModels", "AttachStatus");
        }
    }
}
