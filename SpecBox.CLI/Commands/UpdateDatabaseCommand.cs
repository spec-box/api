using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpecBox.Migrations;
using Spectre.Console;
using Spectre.Console.Cli;
using ThinkingHome.Migrator;
using ThinkingHome.Migrator.Providers.PostgreSQL;

namespace SpecBox.CLI.Commands;

internal sealed class UpdateDatabaseCommand(IConfigurationRoot configuration) : Command
{
    private static ILoggerFactory CreateLoggerFactory(bool verbose)
    {
        var logLevel = verbose ? LogLevel.Trace : LogLevel.Information;

        return LoggerFactory.Create(builder =>
            builder.AddFilter("Default", logLevel).AddConsole());
    }

    private void ApplyMigrations()
    {
        using var loggerFactory = CreateLoggerFactory(false);
        var logger = loggerFactory.CreateLogger("migrate-database");

        string? cstring = configuration.GetConnectionString("default");

        var factory = new PostgreSQLProviderFactory();
        var asm = typeof(Migration_001_Project).Assembly;

        using var migrator = new Migrator(factory.CreateProvider(cstring, logger), asm, logger);

        migrator.Migrate();
    }

    public override int Execute([NotNull] CommandContext context)
    {
        ApplyMigrations();
        AnsiConsole.MarkupLine("[green3]Database is up to date[/]");
        return 0;
    }
}
