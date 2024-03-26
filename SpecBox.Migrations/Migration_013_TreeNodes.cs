using System.Data;
using ThinkingHome.Migrator.Framework;

namespace SpecBox.Migrations;

[Migration(13)]
public class Migration_013_TreeNodes : Migration
{
    public override void Apply()
    {
        Database.AddColumn("TreeNode", new Column("AmountProblem", DbType.Int32, ColumnProperty.NotNull, 0));
        Database.ExecuteFromResource(GetType().Assembly, "SpecBox.Migrations.Resources.013_MergeExportedData.sql");
        Database.ExecuteFromResource(GetType().Assembly, "SpecBox.Migrations.Resources.013_BuildTree.sql");
    }
}
