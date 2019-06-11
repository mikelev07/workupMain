namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StartDateBackUp : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.CustomViewModels", "StartDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.CustomViewModels", "StartDate", c => c.DateTime());
        }
    }
}
