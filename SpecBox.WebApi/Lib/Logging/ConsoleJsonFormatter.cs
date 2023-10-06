using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace SpecBox.WebApi.Lib.Logging;

public class ConsoleJsonFormatter : ConsoleFormatter, IDisposable
{
    private readonly IDisposable? optionsReloadToken;
    private ConsoleFormatterOptions formatterOptions;

    public ConsoleJsonFormatter(IOptionsMonitor<ConsoleFormatterOptions> options)
        // Case insensitive
        : base("SpecBoxJson") =>
        (optionsReloadToken, formatterOptions) =
        (options.OnChange(ReloadLoggerOptions), options.CurrentValue);

    private void ReloadLoggerOptions(ConsoleFormatterOptions options) => formatterOptions = options;

    public override void Write<TState>(
        in LogEntry<TState> logEntry,
        IExternalScopeProvider? scopeProvider,
        TextWriter textWriter)
    {
        textWriter.Write("{");

        var level = MapLevel(logEntry.LogLevel);
        if (level.HasValue)
        {
            textWriter.Write($"\"levelStr\":\"{level.Value.name}\",");
            textWriter.Write($"\"level\":\"{level.Value.value}\",");
        }

        var timestamp = DateTime.UtcNow.ToString("O");
        textWriter.Write($"\"@timestamp\":\"{timestamp}\",");

        var message = logEntry.Formatter.Invoke(logEntry.State, null);

        if (logEntry.Exception != null)
        {
            message += "\n" + logEntry.Exception;
        }

        var encodedMessage = JsonEncodedText.Encode(message);
        textWriter.Write($"\"message\":\"{encodedMessage}\",");

        var encodedCategory = JsonEncodedText.Encode(logEntry.Category);
        textWriter.Write($"\"loggerName\":\"{encodedCategory}\"");
        
        textWriter.WriteLine("}");
    }

    private (string name, int value)? MapLevel(LogLevel level)
    {
        switch (level)
        {
            case LogLevel.Trace:
                return ("TRACE", 10);
            case LogLevel.Debug:
                return ("DEBUG", 20);
            case LogLevel.Information:
                return ("INFO", 30);
            case LogLevel.Warning:
                return ("WARN", 40);
            case LogLevel.Error:
                return ("ERROR", 50);
            case LogLevel.Critical:
                return ("FATAL", 60);
        }

        return null;
    }

    public void Dispose() => optionsReloadToken?.Dispose();
}
