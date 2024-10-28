using SpecBox.Domain;
using SpecBox.Domain.Model;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SpecBox.CLI.Commands;

public class ProjectAddCommand(SpecBoxDbContext db) : Command<ProjectAddCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<code>")] public string Code { get; set; }

        [CommandArgument(1, "<title>")] public string Title { get; set; }

        [CommandOption("-d|--description <description>")]
        public string? Description { get; set; }

        [CommandOption("-r|--repo <repoUrl>")] public string? RepositoryUrl { get; set; }
    }

    public override int Execute(CommandContext context, Settings args)
    {
        using var tran = db.Database.BeginTransaction();

        try
        {
            if (db.Projects.Any(x => x.Code == args.Code))
            {
                throw new ArgumentException($"Project \"{args.Code}\" already exists!");
            }

            var project = new Project
            {
                Id = Guid.NewGuid(),
                Code = args.Code,
                Title = args.Title,
                Description = args.Description,
                RepositoryUrl = args.RepositoryUrl,
            };

            db.Projects.Add(project);
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