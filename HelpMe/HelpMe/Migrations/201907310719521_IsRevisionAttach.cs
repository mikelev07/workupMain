namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IsRevisionAttach : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AttachModels", "IsRevision", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AttachModels", "IsRevision");
        }
    }
}
