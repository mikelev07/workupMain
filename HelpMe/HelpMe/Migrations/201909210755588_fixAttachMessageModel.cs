namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixAttachMessageModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MessageAttaches", "AttachName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.MessageAttaches", "AttachName");
        }
    }
}
