using System.Data;
using System.Reflection;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;
using ForeignKeyConstraint = ThinkingHome.Migrator.Framework.ForeignKeyConstraint;

namespace SpecBox.Migrations;

[Migration(6)]
public class Migration_006_Stat : Migration
{
    public override void Apply()
    {
        Database.AddTable("AutotestsStat",
            new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey, "gen_random_uuid()"),
            new Column("ProjectId", DbType.Guid, ColumnProperty.NotNull),
            new Column("Timestamp", DbType.DateTime, ColumnProperty.NotNull),
            new Column("Duration", DbType.Int32, ColumnProperty.NotNull),
            new Column("AssertionsCount", DbType.Int32, ColumnProperty.NotNull)
        );

        Database.AddForeignKey("FK_AutotestsStat_ProjectId", "AutotestsStat", "ProjectId", "Project", "Id");

        Database.AddTable("AssertionsStat",
            new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey, "gen_random_uuid()"),
            new Column("ProjectId", DbType.Guid, ColumnProperty.NotNull),
            new Column("Timestamp", DbType.DateTime, ColumnProperty.NotNull),
            new Column("TotalCount", DbType.Int32, ColumnProperty.NotNull),
            new Column("AutomatedCount", DbType.Int32, ColumnProperty.NotNull)
        );
        
        Database.AddForeignKey("FK_AssertionsStat_ProjectId", "AssertionsStat", "ProjectId", "Project", "Id");
    }
}
