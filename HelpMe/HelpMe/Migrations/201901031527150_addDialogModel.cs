namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addDialogModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ChatDialogs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        UserFromId = c.String(maxLength: 128),
                        UserToId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserFromId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserToId, cascadeDelete: true)
                .Index(t => t.UserFromId)
                .Index(t => t.UserToId);
            
            AddColumn("dbo.MessageStoreViewModels", "ChatDialogId", c => c.Int());
            CreateIndex("dbo.MessageStoreViewModels", "ChatDialogId");
            AddForeignKey("dbo.MessageStoreViewModels", "ChatDialogId", "dbo.ChatDialogs", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ChatDialogs", "UserToId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ChatDialogs", "UserFromId", "dbo.AspNetUsers");
            DropForeignKey("dbo.MessageStoreViewModels", "ChatDialogId", "dbo.ChatDialogs");
            DropIndex("dbo.MessageStoreViewModels", new[] { "ChatDialogId" });
            DropIndex("dbo.ChatDialogs", new[] { "UserToId" });
            DropIndex("dbo.ChatDialogs", new[] { "UserFromId" });
            DropColumn("dbo.MessageStoreViewModels", "ChatDialogId");
            DropTable("dbo.ChatDialogs");
        }
    }
}
