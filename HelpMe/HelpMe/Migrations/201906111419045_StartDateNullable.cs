namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StartDateNullable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.CustomViewModels", "StartDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.CustomViewModels", "StartDate", c => c.DateTime(nullable: false));
        }
    }
}
