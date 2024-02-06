using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace SpecBox.Migrations;

[Migration(10)]
public class Migration_010_FixTreeBuilder : Migration
{
    public override void Apply()
    {
        Database.ChangeColumn("TreeNode", "Title", DbType.String.WithSize(400), false);
        Database.ExecuteFromResource(GetType().Assembly, "SpecBox.Migrations.Resources.010_BuildTree.sql");
    }
}
