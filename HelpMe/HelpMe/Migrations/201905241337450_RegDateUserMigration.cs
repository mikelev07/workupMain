namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RegDateUserMigration : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "RegistrationDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.UserViewModels", "RegistrationDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserViewModels", "RegistrationDate");
            DropColumn("dbo.AspNetUsers", "RegistrationDate");
        }
    }
}
