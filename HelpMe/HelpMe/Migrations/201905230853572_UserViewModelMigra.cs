namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserViewModelMigra : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserViewModels", "IsOnline", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserViewModels", "IsOnline");
        }
    }
}
