using System.Data;
using ThinkingHome.Migrator.Framework;

namespace SpecBox.Migrations;

[Migration(11)]
public class Migration_011_FeatureTypeAndAssertionState : Migration
{
    public override void Apply()
    {
        Database.AddColumn("Feature", new Column("FeatureType", DbType.Int32, ColumnProperty.Null));

        Database.AddColumn("Assertion", new Column("AutomationState", DbType.Int32, ColumnProperty.NotNull, 0));

        Database.Update("Assertion", new[] { "AutomationState" }, new[] { "1" }, "\"IsAutomated\" = true");

        Database.RemoveColumn("Assertion", "IsAutomated");
    }
}
