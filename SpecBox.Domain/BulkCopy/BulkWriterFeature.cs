using Npgsql;
using NpgsqlTypes;

namespace SpecBox.Domain.BulkCopy;

public class BulkWriterFeature : BulkWriter
{
    private const string COMMAND =
        "COPY \"ExportFeature\" (\"ExportId\", \"Code\",\"Title\",\"Description\", \"FilePath\") FROM STDIN (FORMAT BINARY)";

    public BulkWriterFeature(NpgsqlConnection connection) : base(COMMAND, connection)
    {
    }

    public async Task AddFeature(
        Guid exportId,
        string code,
        string title,
        string? description,
        string? filePath)
    {
        await Writer.StartRowAsync();
        await Writer.WriteAsync(exportId, NpgsqlDbType.Uuid);
        await Writer.WriteAsync(code, NpgsqlDbType.Text);
        await Writer.WriteAsync(title, NpgsqlDbType.Text);
        await Writer.WriteAsync(description, NpgsqlDbType.Text);
        await Writer.WriteAsync(filePath, NpgsqlDbType.Text);
    }
}
