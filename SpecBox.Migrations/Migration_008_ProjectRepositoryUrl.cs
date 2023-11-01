using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace SpecBox.Migrations;

[Migration(8)]
public class Migration_008_ProjectRepositoryUrl : Migration
{
    public override void Apply()
    {
        Database.AddColumn("Project", new Column("RepositoryUrl", DbType.String.WithSize(400), ColumnProperty.Null));
    }
}
