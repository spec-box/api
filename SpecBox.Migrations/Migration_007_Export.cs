using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace SpecBox.Migrations;

[Migration(7)]
public class Migration_007_Export : Migration
{
    public override void Apply()
    {
        // export
        Database.AddTable("Export",
            new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey, "gen_random_uuid()"),
            new Column("ProjectId", DbType.Guid, ColumnProperty.NotNull),
            new Column("Timestamp", DbType.DateTime, ColumnProperty.NotNull)
        );

        // export feature
        Database.AddTable("ExportFeature",
            new Column("ExportId", DbType.Guid, ColumnProperty.NotNull),
            new Column("Code", DbType.String.WithSize(255), ColumnProperty.NotNull),
            new Column("Title", DbType.String.WithSize(400), ColumnProperty.NotNull),
            new Column("Description", DbType.String.WithSize(int.MaxValue), ColumnProperty.Null),
            new Column("FilePath", DbType.String.WithSize(int.MaxValue), ColumnProperty.Null)
        );

        Database.AddPrimaryKey("PK_ExportFeature", "ExportFeature", "ExportId", "Code");
        Database.AddForeignKey("FK_ExportFeature_ExportId", "ExportFeature", "ExportId", "Export", "Id");

        // export assertion
        Database.AddTable("ExportAssertion",
            new Column("ExportId", DbType.Guid, ColumnProperty.NotNull),
            new Column("FeatureCode", DbType.String.WithSize(255), ColumnProperty.NotNull),
            new Column("GroupTitle", DbType.String.WithSize(400), ColumnProperty.NotNull),
            new Column("Title", DbType.String.WithSize(400), ColumnProperty.NotNull),
            new Column("Description", DbType.String.WithSize(int.MaxValue), ColumnProperty.Null),
            new Column("IsAutomated", DbType.Boolean, ColumnProperty.NotNull, false)
        );

        Database.AddPrimaryKey(
            "PK_ExportAssertion",
            "ExportAssertion",
            "ExportId", "FeatureCode", "GroupTitle", "Title"
        );

        Database.AddForeignKey("FK_ExportAssertion_ExportId", "ExportAssertion", "ExportId", "Export", "Id");

        // export feature attribute
        Database.AddTable("ExportFeatureAttribute",
            new Column("ExportId", DbType.Guid, ColumnProperty.NotNull),
            new Column("FeatureCode", DbType.String.WithSize(255), ColumnProperty.NotNull),
            new Column("AttributeCode", DbType.String.WithSize(255), ColumnProperty.NotNull),
            new Column("AttributeValueCode", DbType.String.WithSize(255), ColumnProperty.NotNull)
        );

        Database.AddPrimaryKey(
            "PK_ExportFeatureAttribute",
            "ExportFeatureAttribute",
            "ExportId", "FeatureCode", "AttributeCode", "AttributeValueCode"
        );

        Database.AddForeignKey("FK_ExportFeatureAttribute_ExportId", "ExportFeatureAttribute", "ExportId", "Export", "Id");
        
        
        Database.ExecuteFromResource(GetType().Assembly, "SpecBox.Migrations.Resources.007_MergeExportedData.sql");
    }
}
