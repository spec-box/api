using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace SpecBox.Migrations;

[Migration(17)]
public class Migration_017_AssertionDetailsUrl : Migration
{
    public override void Apply()
    {
        Database.AddColumn("Assertion", new Column("DetailsUrl", DbType.String.WithSize(int.MaxValue), ColumnProperty.Null));
        Database.AddColumn("ExportAssertion", new Column("DetailsUrl", DbType.String.WithSize(int.MaxValue), ColumnProperty.Null));
        
        Database.ExecuteFromResource(GetType().Assembly, "SpecBox.Migrations.Resources.017_MergeExportedData.sql");
    }
}
