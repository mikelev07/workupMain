namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DbContekst : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserSkill",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        SkillId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.SkillId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.Skills", t => t.SkillId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.SkillId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserSkill", "SkillId", "dbo.Skills");
            DropForeignKey("dbo.UserSkill", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.UserSkill", new[] { "SkillId" });
            DropIndex("dbo.UserSkill", new[] { "UserId" });
            DropTable("dbo.UserSkill");
        }
    }
}
