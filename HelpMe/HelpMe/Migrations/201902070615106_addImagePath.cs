namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addImagePath : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "ImagePath", c => c.String());
            DropColumn("dbo.AspNetUsers", "UserPhoto");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "UserPhoto", c => c.Binary());
            DropColumn("dbo.AspNetUsers", "ImagePath");
        }
    }
}
