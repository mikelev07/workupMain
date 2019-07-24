namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TransactionProperties2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Transactions", "CustomId", c => c.Int(nullable: false));
            AddColumn("dbo.Transactions", "Date", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Transactions", "Status", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Transactions", "Status", c => c.String());
            DropColumn("dbo.Transactions", "Date");
            DropColumn("dbo.Transactions", "CustomId");
        }
    }
}
