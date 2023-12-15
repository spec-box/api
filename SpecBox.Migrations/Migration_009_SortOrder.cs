using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace SpecBox.Migrations;

[Migration(9)]
public class Migration_009_SortOrder : Migration
{
    public override void Apply()
    {
        Database.AddColumn("AttributeValue", new Column("SortOrder", DbType.Int32, ColumnProperty.Null));
        Database.AddColumn("TreeNode", new Column("SortOrder", DbType.Int32, ColumnProperty.Null));

        Database.ExecuteFromResource(GetType().Assembly, "SpecBox.Migrations.Resources.009_BuildTree.sql");
    }
}
