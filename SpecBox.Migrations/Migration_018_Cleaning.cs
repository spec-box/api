using ThinkingHome.Migrator.Framework;

namespace SpecBox.Migrations;

[Migration(18)]
public class Migration_018_Cleaning : Migration
{
    public override void Apply()
    {
        Database.ExecuteFromResource(GetType().Assembly, "SpecBox.Migrations.Resources.018_MergeExportedData.sql");
    }
}
