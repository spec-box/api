using Spectre.Console.Cli;

namespace SpecBox.CLI;

public class Program
{
    public static int Main(string[] args)
    {
        var app = new CommandApp();
        
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