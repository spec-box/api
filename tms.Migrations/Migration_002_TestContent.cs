using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace tms.Migrations;

[Migration(2)]
public class Migration_002_TestContent : Migration
{
    public override void Apply()
    {
        Database.AddTable("Entity",
            new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey, "gen_random_uuid()"),
            new Column("Code", DbType.String.WithSize(255), ColumnProperty.NotNull),
            new Column("ProjectId", DbType.Guid, ColumnProperty.NotNull),
            new Column("Title", DbType.String.WithSize(400), ColumnProperty.NotNull),
            new Column("Description", DbType.String.WithSize(int.MaxValue), ColumnProperty.Null)
        );

        Database.AddUniqueConstraint("UK_Entity_Code_ProjectId", "Entity", "Code", "ProjectId");
        Database.AddForeignKey("FK_Entity_ProjectId", "Entity", "ProjectId", "Project", "Id");

        Database.AddTable("AssertionGroup",
            new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey, "gen_random_uuid()"),
            new Column("EntityId", DbType.Guid, ColumnProperty.NotNull),
            new Column("Title", DbType.String.WithSize(400), ColumnProperty.NotNull),
            new Column("Description", DbType.String.WithSize(int.MaxValue), ColumnProperty.Null)
        );

        Database.AddForeignKey("FK_AssertionGroup_EntityId", "AssertionGroup", "EntityId", "Entity", "Id");

        Database.AddTable("Assertion",
            new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey, "gen_random_uuid()"),
            new Column("AssertionGroupId", DbType.Guid, ColumnProperty.NotNull),
            new Column("Title", DbType.String.WithSize(400), ColumnProperty.NotNull),
            new Column("Description", DbType.String.WithSize(int.MaxValue), ColumnProperty.Null),
            new Column("IsAutomated", DbType.Boolean, ColumnProperty.NotNull, false)
        );

        Database.AddForeignKey("FK_Assertion_AssertionGroupId", "Assertion", "AssertionGroupId", "AssertionGroup",
            "Id");
    }
}
