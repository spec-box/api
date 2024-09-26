using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace SpecBox.Migrations
{
    [Migration(17)]
    public class Migration_017_ExampleProject: Migration
    {
        public override void Apply()
        {
            Database.Insert("Project", new[] {"Code", "Title", "Description"}, new[] {"id", "id", "id"});
        }

        public override void Revert()
        {
	    Database.Delete("Project", "code='id'");
        }
	
    }
}
