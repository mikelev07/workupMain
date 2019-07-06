namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CommentCreationDateMigra : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CommentViewModels", "CreationDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CommentViewModels", "CreationDate");
        }
    }
}
