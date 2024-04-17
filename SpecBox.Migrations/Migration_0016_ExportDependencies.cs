using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;
using ForeignKeyConstraint = ThinkingHome.Migrator.Framework.ForeignKeyConstraint;

namespace SpecBox.Migrations;

[Migration(16)]
public class Migration_0016_ExportDependencies : Migration
{
    public override void Apply()
    {
        // export dependencies
        Database.AddTable("ExportFeatureDependency",
            new Column("ExportId", DbType.Guid, ColumnProperty.NotNull),
            // фича, которая зависит от другой
            new Column("SourceFeatureCode", DbType.String.WithSize(255), ColumnProperty.NotNull),
            // фича, от которой зависит целевая фича
            new Column("DependencyFeatureCode", DbType.String.WithSize(255), ColumnProperty.NotNull)
        );

        Database.AddPrimaryKey(
            "PK_ExportFeatureDependency",
            "ExportFeatureDependency",
            "ExportId", "SourceFeatureCode", "DependencyFeatureCode");

        Database.AddForeignKey(
            "FK_ExportFeatureDependency_ExportId",
            "ExportFeatureDependency", "ExportId", "Export", "Id");

        Database.AddColumn("Export", new Column("Message", DbType.String.WithSize(int.MaxValue), ColumnProperty.Null));
        
        Database.AddTable("FeatureDependency",
            new Column("SourceFeatureId", DbType.Guid),
            new Column("DependencyFeatureId", DbType.Guid)
        );

        Database.AddForeignKey(
            "FK_FeatureDependency_SourceFeatureId",
            "FeatureDependency", "SourceFeatureId",
            "Feature", "Id", ForeignKeyConstraint.Cascade);
        
        Database.AddForeignKey(
            "FK_FeatureDependency_DependencyFeatureId",
            "FeatureDependency", "DependencyFeatureId",
            "Feature", "Id", ForeignKeyConstraint.Cascade);
        
        Database.ExecuteFromResource(GetType().Assembly, "SpecBox.Migrations.Resources.016_MergeExportedData.sql");
    }
}
