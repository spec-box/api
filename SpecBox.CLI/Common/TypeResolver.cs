using Spectre.Console.Cli;

namespace SpecBox.CLI.Common;

public sealed class TypeResolver(IServiceProvider provider) : ITypeResolver
{
    private readonly IServiceProvider provider = provider ?? throw new ArgumentNullException(nameof(provider));

    public object? Resolve(Type? type)
    {
        return type == null ? null : provider.GetService(type);
    }
}