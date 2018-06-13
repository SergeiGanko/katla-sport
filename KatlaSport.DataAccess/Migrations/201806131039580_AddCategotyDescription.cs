namespace KatlaSport.DataAccess.Migrations
{
    using System.Data.Entity.Migrations;

    /// <summary>
    /// Add Category Description migration.
    /// </summary>
    public partial class AddCategotyDescription : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.product_categories", "category_description", c => c.String(maxLength: 300));
        }

        public override void Down()
        {
            DropColumn("dbo.product_categories", "category_description");
        }
    }
}
