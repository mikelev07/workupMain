namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DialogStatusAwayMigration : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.ChatDialogs", "Status");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ChatDialogs", "Status", c => c.Int(nullable: false));
        }
    }
}
