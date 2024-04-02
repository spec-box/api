using System.Data;
using ThinkingHome.Migrator.Framework;

namespace SpecBox.Migrations;

[Migration(12)]
public class Migration_012_ExportTables : Migration
{
    public override void Apply()
    {
        Database.AddColumn("ExportFeature", new Column("FeatureType", DbType.Int32, ColumnProperty.Null));
        
        Database.AddColumn("ExportAssertion", new Column("AutomationState", DbType.Int32, ColumnProperty.NotNull, 0));

        Database.RemoveColumn("ExportAssertion", "IsAutomated");
    }
}
