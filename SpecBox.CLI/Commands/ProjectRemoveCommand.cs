using SpecBox.Domain;
using SpecBox.Domain.Model;
using Spectre.Console.Cli;

namespace SpecBox.CLI.Commands;

public class ProjectRemoveCommand(SpecBoxDbContext db) : Command<ProjectRemoveCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<code>")] public string Code { get; set; }
    }

    public override int Execute(CommandContext context, Settings args)
    {
        using var tran = db.Database.BeginTransaction();

        try
        {
            var proj = db.Projects.FirstOrDefault(p => p.Code == args.Code);

            if (proj == null)
            {
                throw new ArgumentException($"Project \"{args.Code}\" is not exists!");
            }

            db.Assertions.RemoveRange(
                db.Assertions.Where(a => a.AssertionGroup.Feature.Project.Code == args.Code));

            db.AssertionGroups.RemoveRange(
                db.AssertionGroups.Where(g => g.Feature.Project.Code == args.Code));

            db.Features.RemoveRange(
                db.Features.Where(f => f.Project.Code == args.Code));

            db.Projects.Remove(proj);
            db.SaveChanges();
            tran.Commit();

            return 0;
        }
        finally
        {
            tran.Rollback();
        }
    }
}