using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SpecBox.Domain;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SpecBox.CLI;

public class ProjectListCommand : Command
{
    public override int Execute(CommandContext context)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true)
            .Build();
        
        string? cstring = configuration.GetConnectionString("default");

        var contextBuilder = new DbContextOptionsBuilder<SpecBoxDbContext>();
        contextBuilder.UseNpgsql(cstring);

        using var db = new SpecBoxDbContext(contextBuilder.Options);

        var projects = db.Projects.ToList();

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

        return 0;
    }
}