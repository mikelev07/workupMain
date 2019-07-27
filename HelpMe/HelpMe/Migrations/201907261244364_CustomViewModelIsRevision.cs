namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CustomViewModelIsRevision : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomViewModels", "IsRevision", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CustomViewModels", "IsRevision");
        }
    }
}
