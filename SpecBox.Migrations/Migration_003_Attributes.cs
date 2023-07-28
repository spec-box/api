using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;
using ForeignKeyConstraint = ThinkingHome.Migrator.Framework.ForeignKeyConstraint;

namespace SpecBox.Migrations;

[Migration(3)]
public class Migration_003_Attributes : Migration
{
    public override void Apply()
    {
        Database.AddTable("Attribute",
            new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey, "gen_random_uuid()"),
            new Column("Code", DbType.String.WithSize(255), ColumnProperty.NotNull),
            new Column("ProjectId", DbType.Guid, ColumnProperty.NotNull),
            new Column("Title", DbType.String.WithSize(400), ColumnProperty.NotNull)
        );

        Database.AddUniqueConstraint("UK_Attribute_Code_ProjectId", "Attribute", "Code", "ProjectId");
        Database.AddForeignKey("FK_Attribute_ProjectId", "Attribute", "ProjectId", "Project", "Id");

        Database.AddTable("AttributeValue",
            new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey, "gen_random_uuid()"),
            new Column("Code", DbType.String.WithSize(255), ColumnProperty.NotNull),
            new Column("AttributeId", DbType.Guid, ColumnProperty.NotNull),
            new Column("Title", DbType.String.WithSize(400), ColumnProperty.NotNull)
        );

        Database.AddUniqueConstraint("UK_AttributeValue_Code_AttributeId", "AttributeValue", "Code", "AttributeId");
        Database.AddForeignKey("FK_AttributeValue_AttributeId", "AttributeValue", "AttributeId", "Attribute", "Id",
            ForeignKeyConstraint.Cascade);

        Database.AddTable("FeatureAttributeValue",
            new Column("AttributeValueId", DbType.Guid, ColumnProperty.PrimaryKey),
            new Column("FeatureId", DbType.Guid, ColumnProperty.PrimaryKey)
        );

        Database.AddForeignKey("FK_FeatureAttributeValue_AttributeId", "FeatureAttributeValue", "AttributeValueId",
            "AttributeValue", "Id", ForeignKeyConstraint.Cascade);
        Database.AddForeignKey("FK_FeatureAttributeValue_FeatureId", "FeatureAttributeValue", "FeatureId", "Feature",
            "Id", ForeignKeyConstraint.Cascade);
    }
}
