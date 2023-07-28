using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace SpecBox.Migrations
{
    [Migration(1)]
    public class Migration_001_Project : Migration
    {
        public override void Apply()
        {
            Database.AddTable("Project",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey, "gen_random_uuid()"),
                new Column("Code", DbType.String.WithSize(255), ColumnProperty.NotNull),
                new Column("Title", DbType.String.WithSize(255), ColumnProperty.NotNull),
                new Column("Description", DbType.String.WithSize(int.MaxValue), ColumnProperty.Null));

            Database.AddUniqueConstraint("UK_Project_Code", "Project", "Code");
        }

        public override void Revert()
        {
            Database.RemoveConstraint("Project", "UK_Project_Code");
            Database.RemoveTable("Project");
        }
    }
}
