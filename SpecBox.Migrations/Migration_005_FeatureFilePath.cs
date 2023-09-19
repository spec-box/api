using System;
using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace SpecBox.Migrations;

[Migration(5)]
public class Migration_005_FeatureFilePath : Migration
{
    public override void Apply()
    {
        Database.AddColumn("Feature",
            new Column("FilePath", DbType.String.WithSize(Int32.MaxValue), ColumnProperty.Null));
    }
}
