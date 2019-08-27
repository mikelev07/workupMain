namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DateTimeNullableMigra : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.MainAttachModels", "DownloadDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.MainAttachModels", "DownloadDate", c => c.DateTime(nullable: false));
        }
    }
}
