namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class propsToUserMigra : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "IsOnline", c => c.Boolean(nullable: false));
            AddColumn("dbo.AspNetUsers", "IsBusy", c => c.Boolean(nullable: false));
            AddColumn("dbo.AspNetUsers", "Status", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "Status");
            DropColumn("dbo.AspNetUsers", "IsBusy");
            DropColumn("dbo.AspNetUsers", "IsOnline");
        }
    }
}
