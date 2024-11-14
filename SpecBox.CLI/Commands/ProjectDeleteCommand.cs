using SpecBox.Domain;
using SpecBox.Domain.Model;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SpecBox.CLI.Commands;

public class ProjectDeleteCommand(SpecBoxDbContext db) : Command<ProjectDeleteCommand.Settings>
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
            var code = args.Code;

            var proj = db.Projects.FirstOrDefault(p => p.Code == code);

            if (proj == null)
            {
                throw new ArgumentException($"Project \"{code}\" is not exists!");
            }

            db.Assertions.RemoveRange(
                db.Assertions.Where(a => a.AssertionGroup.Feature.Project.Code == code));

            db.AssertionGroups.RemoveRange(
                db.AssertionGroups.Where(g => g.Feature.Project.Code == code));

            db.TreeNodes.RemoveRange(
                db.TreeNodes.Where(n => n.Tree.Project.Code == code || n.Feature.Project.Code == code));

            db.AttributeValues.RemoveRange(
                db.AttributeValues.Where(a => a.Attribute.Project.Code == code));

            db.AttributeGroupOrders.RemoveRange(
                db.AttributeGroupOrders.Where(a => a.Attribute.Project.Code == code));

            db.Attributes.RemoveRange(
                db.Attributes.Where(a => a.Project.Code == code));

            db.Trees.RemoveRange(
                db.Trees.Where(a => a.Project.Code == code));

            db.AssertionsStat.RemoveRange(
                db.AssertionsStat.Where(s => s.Project.Code == code));
            db.AutotestsStat.RemoveRange(
                db.AutotestsStat.Where(s => s.Project.Code == code));

            db.Features.RemoveRange(
                db.Features.Where(f => f.Project.Code == code));

            db.Projects.Remove(proj);
            db.SaveChanges();

            tran.Commit();

            AnsiConsole.MarkupLine($"[green3]Project \"{code}\" was deleted successfully[/]");
            return 0;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }
}
