using System.Data;
using System.Reflection;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;
using ForeignKeyConstraint = ThinkingHome.Migrator.Framework.ForeignKeyConstraint;

namespace SpecBox.Migrations;

[Migration(4)]
public class Migration_004_Trees : Migration
{
    public override void Apply()
    {
        Database.AddTable("Tree",
            new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey, "gen_random_uuid()"),
            new Column("Code", DbType.String.WithSize(255), ColumnProperty.NotNull),
            new Column("ProjectId", DbType.Guid, ColumnProperty.NotNull),
            new Column("Title", DbType.String.WithSize(400), ColumnProperty.NotNull)
        );

        Database.AddUniqueConstraint("UK_Tree_Code_ProjectId", "Tree", "Code", "ProjectId");
        Database.AddForeignKey("FK_Tree_ProjectId", "Tree", "ProjectId", "Project", "Id");

        Database.AddTable("AttributeGroupOrder",
            new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey, "gen_random_uuid()"),
            new Column("Order", DbType.Int32, ColumnProperty.NotNull),
            new Column("TreeId", DbType.Guid, ColumnProperty.NotNull),
            new Column("AttributeId", DbType.Guid, ColumnProperty.NotNull)
        );
        Database.AddForeignKey("FK_AttributeGroupOrder_TreeId", "AttributeGroupOrder", "TreeId", "Tree", "Id", ForeignKeyConstraint.Cascade);
        Database.AddForeignKey("FK_AttributeGroupOrder_AttributeId", "AttributeGroupOrder", "AttributeId", "Attribute", "Id");

        Database.AddTable("TreeNode",
            new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey, "gen_random_uuid()"),
            new Column("Title", DbType.String.WithSize(400), ColumnProperty.NotNull),
            new Column("Amount", DbType.Int32, ColumnProperty.NotNull),
            new Column("AmountAutomated", DbType.Int32, ColumnProperty.NotNull),
            new Column("AttributeValueId", DbType.Guid),
            new Column("FeatureId", DbType.Guid),
            new Column("TreeId", DbType.Guid, ColumnProperty.NotNull),
            new Column("ParentId", DbType.Guid)
        );
        Database.AddForeignKey("FK_TreeNode_TreeId", "TreeNode", "TreeId", "Tree", "Id", ForeignKeyConstraint.Cascade);
        Database.AddForeignKey("FK_TreeNode_AttributeValueId", "TreeNode", "AttributeValueId", "AttributeValue", "Id");
        Database.AddForeignKey("FK_TreeNode_FeatureId", "TreeNode", "FeatureId", "Feature", "Id");
        Database.AddForeignKey("FK_TreeNode_ParentId", "TreeNode", "ParentId", "TreeNode", "Id");

        Database.ExecuteFromResource(Assembly.GetExecutingAssembly(), "SpecBox.Migrations.Resources.BuildTree.sql");
    }
}
