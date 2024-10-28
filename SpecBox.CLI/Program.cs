using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpecBox.CLI.Commands;
using SpecBox.CLI.Common;
using SpecBox.Domain;
using Spectre.Console.Cli;

namespace SpecBox.CLI;

public class Program
{
    public static int Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true)
            .AddEnvironmentVariables()
            .Build();

        string? cstring = configuration.GetConnectionString("default");
        
        var deps = new ServiceCollection();
        deps.AddNpgsql<SpecBoxDbContext>(cstring);

        var registrar = new TypeRegistrar(deps);
        var app = new CommandApp(registrar);

        app.Configure(config =>
        {
            config.AddCommand<UpdateDatabaseCommand>("migrate");
            config.AddBranch("project", cfgProject =>
            {
                cfgProject.AddCommand<ProjectListCommand>("list");
                cfgProject.AddCommand<ProjectAddCommand>("add");
                cfgProject.AddCommand<ProjectRemoveCommand>("remove");
            });
        });

        return app.Run(args);
    }
}