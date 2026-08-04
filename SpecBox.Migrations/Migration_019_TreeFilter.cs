using ThinkingHome.Migrator.Framework;

namespace SpecBox.Migrations;

[Migration(19)]
public class Migration_019_TreeFilter : Migration
{
    public override void Apply()
    {
        Database.ExecuteFromResource(GetType().Assembly, "SpecBox.Migrations.Resources.019_FilterTree.sql");
    }
}
