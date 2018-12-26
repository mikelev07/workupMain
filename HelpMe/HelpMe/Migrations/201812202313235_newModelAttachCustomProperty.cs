namespace HelpMe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class newModelAttachCustomProperty : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.AttachModels", name: "CustomViewModel_Id", newName: "CustomViewModelId");
            RenameIndex(table: "dbo.AttachModels", name: "IX_CustomViewModel_Id", newName: "IX_CustomViewModelId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.AttachModels", name: "IX_CustomViewModelId", newName: "IX_CustomViewModel_Id");
            RenameColumn(table: "dbo.AttachModels", name: "CustomViewModelId", newName: "CustomViewModel_Id");
        }
    }
}
