using SpecBox.Domain;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SpecBox.CLI.Commands;

public class ProjectListCommand(SpecBoxDbContext db) : Command
{
    public override int Execute(CommandContext context)
    {
        var projects = db.Projects.ToList();

        if (projects.Any())
        {
            var table = new Table();

            // Add some columns
            table.AddColumn("Code");
            table.AddColumn("Title");
            table.AddColumn("Description");

            foreach (var project in projects)
            {
                table.AddRow(project.Code, project.Title, project.Description ?? string.Empty);
            }

            AnsiConsole.Write(table);
        }
        else
        {
            AnsiConsole.MarkupLine("[orange3]No projects found![/]");
        }

        return 0;
    }
}