namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class newMigra1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomViewModels", "StartDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.CustomViewModels", "SkillId", c => c.Int());
            AddColumn("dbo.CustomViewModels", "Price", c => c.Int(nullable: false));
            CreateIndex("dbo.CustomViewModels", "SkillId");
            AddForeignKey("dbo.CustomViewModels", "SkillId", "dbo.Skills", "Id");
            DropColumn("dbo.CustomViewModels", "MinPrice");
            DropColumn("dbo.CustomViewModels", "MaxPrice");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CustomViewModels", "MaxPrice", c => c.Int(nullable: false));
            AddColumn("dbo.CustomViewModels", "MinPrice", c => c.Int(nullable: false));
            DropForeignKey("dbo.CustomViewModels", "SkillId", "dbo.Skills");
            DropIndex("dbo.CustomViewModels", new[] { "SkillId" });
            DropColumn("dbo.CustomViewModels", "Price");
            DropColumn("dbo.CustomViewModels", "SkillId");
            DropColumn("dbo.CustomViewModels", "StartDate");
        }
    }
}
