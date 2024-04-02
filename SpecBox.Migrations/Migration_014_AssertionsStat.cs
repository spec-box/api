using System.Data;
using ThinkingHome.Migrator.Framework;

namespace SpecBox.Migrations;

[Migration(14)]
public class Migration_014_AssertionsStat : Migration
{
    public override void Apply()
    {
        Database.AddColumn("AssertionsStat", new Column("ProblemCount", DbType.Int32, ColumnProperty.NotNull, 0));
    }
}
