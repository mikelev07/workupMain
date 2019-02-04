namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addOpenCloseDialogStatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ChatDialogs", "Status", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ChatDialogs", "Status");
        }
    }
}
