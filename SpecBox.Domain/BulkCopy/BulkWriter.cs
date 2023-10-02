using Npgsql;

namespace SpecBox.Domain.BulkCopy;

public abstract class BulkWriter: IDisposable, IAsyncDisposable
{
    protected readonly NpgsqlBinaryImporter Writer;

    protected BulkWriter(string copyFromCommand, NpgsqlConnection connection)
    {
        Writer = connection.BeginBinaryImport(copyFromCommand);
    }

    public async Task CompleteAsync()
    {
        await Writer.CompleteAsync();
        await Writer.CloseAsync();
    }
    
    public void Dispose() => Writer.Dispose();
    public ValueTask DisposeAsync() => Writer.DisposeAsync();
}
