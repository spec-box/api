using System.Data;
using ThinkingHome.Migrator.Framework;

namespace SpecBox.Migrations;

[Migration(15)]
public class Migration_015_SortOrders : Migration
{
    public override void Apply()
    {
        Database.AddColumn("ExportAssertion", new Column("GroupSortOrder", DbType.Int32, ColumnProperty.Null));
        Database.AddColumn("ExportAssertion", new Column("AssertionSortOrder", DbType.Int32, ColumnProperty.Null));
        Database.AddColumn("Assertion", new Column("SortOrder", DbType.Int32, ColumnProperty.Null));
        Database.AddColumn("AssertionGroup", new Column("SortOrder", DbType.Int32, ColumnProperty.Null));

        Database.ExecuteFromResource(GetType().Assembly, "SpecBox.Migrations.Resources.015_MergeExportedData.sql");
    }
}
