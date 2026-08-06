using ThinkingHome.Migrator.Framework;

namespace SpecBox.Migrations;

[Migration(21)]
public class Migration_021_TreeFilterDefaultFeatureType : Migration
{
    public override void Apply()
    {
        Database.ExecuteFromResource(GetType().Assembly, "SpecBox.Migrations.Resources.021_FilterTree.sql");
    }
}
