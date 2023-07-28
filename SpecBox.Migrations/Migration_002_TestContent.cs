using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace SpecBox.Migrations;

[Migration(2)]
public class Migration_002_TestContent : Migration
{
    public override void Apply()
    {
        Database.AddTable("Feature",
            new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey, "gen_random_uuid()"),
            new Column("Code", DbType.String.WithSize(255), ColumnProperty.NotNull),
            new Column("ProjectId", DbType.Guid, ColumnProperty.NotNull),
            new Column("Title", DbType.String.WithSize(400), ColumnProperty.NotNull),
            new Column("Description", DbType.String.WithSize(int.MaxValue), ColumnProperty.Null)
        );

        Database.AddUniqueConstraint("UK_Feature_Code_ProjectId", "Feature", "Code", "ProjectId");
        Database.AddForeignKey("FK_Feature_ProjectId", "Feature", "ProjectId", "Project", "Id");

        Database.AddTable("AssertionGroup",
            new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey, "gen_random_uuid()"),
            new Column("FeatureId", DbType.Guid, ColumnProperty.NotNull),
            new Column("Title", DbType.String.WithSize(400), ColumnProperty.NotNull)
        );

        Database.AddForeignKey("FK_AssertionGroup_FeatureId", "AssertionGroup", "FeatureId", "Feature", "Id");

        Database.AddTable("Assertion",
            new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey, "gen_random_uuid()"),
            new Column("AssertionGroupId", DbType.Guid, ColumnProperty.NotNull),
            new Column("Title", DbType.String.WithSize(400), ColumnProperty.NotNull),
            new Column("Description", DbType.String.WithSize(int.MaxValue), ColumnProperty.Null),
            new Column("IsAutomated", DbType.Boolean, ColumnProperty.NotNull, false)
        );

        Database.AddUniqueConstraint("UK_Assertion_Title_AssertionGroupId", "Assertion", "Title", "AssertionGroupId");
        Database.AddForeignKey("FK_Assertion_AssertionGroupId", "Assertion", "AssertionGroupId", "AssertionGroup",
            "Id");
    }

    public override void Revert()
    {
        Database.RemoveConstraint("Assertion", "FK_Assertion_AssertionGroupId");
        Database.RemoveConstraint("Assertion", "UK_Assertion_Title_AssertionGroupId");
        Database.RemoveTable("Assertion");

        Database.RemoveConstraint("AssertionGroup", "FK_AssertionGroup_FeatureId");
        Database.RemoveTable("AssertionGroup");

        Database.RemoveConstraint("Feature", "FK_Feature_ProjectId");
        Database.RemoveConstraint("Feature", "UK_Feature_Code_ProjectId");
        Database.RemoveTable("Feature");
    }
}
