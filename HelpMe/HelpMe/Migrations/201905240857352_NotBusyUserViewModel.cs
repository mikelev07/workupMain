namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NotBusyUserViewModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "IsNotBusy", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserViewModels", "IsNotBusy", c => c.Boolean(nullable: false));
            DropColumn("dbo.AspNetUsers", "IsBusy");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "IsBusy", c => c.Boolean(nullable: false));
            DropColumn("dbo.UserViewModels", "IsNotBusy");
            DropColumn("dbo.AspNetUsers", "IsNotBusy");
        }
    }
}
