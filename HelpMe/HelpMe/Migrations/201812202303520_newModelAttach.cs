namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class newModelAttach : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AttachModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ExecutorPrice = c.Int(nullable: false),
                        UserId = c.String(maxLength: 128),
                        AttachFilePath = c.String(),
                        CustomViewModel_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .ForeignKey("dbo.CustomViewModels", t => t.CustomViewModel_Id)
                .Index(t => t.UserId)
                .Index(t => t.CustomViewModel_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AttachModels", "CustomViewModel_Id", "dbo.CustomViewModels");
            DropForeignKey("dbo.AttachModels", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.AttachModels", new[] { "CustomViewModel_Id" });
            DropIndex("dbo.AttachModels", new[] { "UserId" });
            DropTable("dbo.AttachModels");
        }
    }
}
