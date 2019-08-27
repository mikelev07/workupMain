namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MainAttachmentMigra : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MainAttachModels", "IsDownloaded", c => c.Boolean(nullable: false));
            AddColumn("dbo.MainAttachModels", "DownloadDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.MainAttachModels", "DownloadDate");
            DropColumn("dbo.MainAttachModels", "IsDownloaded");
        }
    }
}
