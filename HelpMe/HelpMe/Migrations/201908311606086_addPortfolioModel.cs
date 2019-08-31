namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addPortfolioModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Portfolios",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(maxLength: 128),
                        IsDownloaded = c.Boolean(nullable: false),
                        DownloadDate = c.DateTime(),
                        AttachFileName = c.String(),
                        AttachFileExtens = c.String(),
                        AttachFilePath = c.String(),
                        AttachStatus = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Portfolios", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.Portfolios", new[] { "UserId" });
            DropTable("dbo.Portfolios");
        }
    }
}
