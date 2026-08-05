using ThinkingHome.Migrator.Framework;

namespace SpecBox.Migrations;

[Migration(20)]
public class Migration_020_TreeFilterExpand : Migration
{
    public override void Apply()
    {
        Database.ExecuteFromResource(GetType().Assembly, "SpecBox.Migrations.Resources.020_FilterTree.sql");
    }
}
