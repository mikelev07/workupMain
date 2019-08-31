namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserPhoneMigration : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserViewModels", "Phone", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserViewModels", "Phone");
        }
    }
}
