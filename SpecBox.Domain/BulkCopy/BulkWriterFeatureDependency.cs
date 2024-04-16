using Npgsql;
using NpgsqlTypes;

namespace SpecBox.Domain.BulkCopy;

public class BulkWriterFeatureDependency : BulkWriter
{
    private const string COMMAND =
        "COPY \"ExportFeatureDependency\" (\"ExportId\", \"SourceFeatureCode\", \"DependencyFeatureCode\") FROM STDIN (FORMAT BINARY)";

    public BulkWriterFeatureDependency(NpgsqlConnection connection) : base(COMMAND, connection)
    {
    }
    
    public async Task AddFeatureDependency(
        Guid exportId,
        string sourceFeatureCode,
        string dependencyFeatureCode)
    {
        await Writer.StartRowAsync();
        await Writer.WriteAsync(exportId, NpgsqlDbType.Uuid);
        await Writer.WriteAsync(sourceFeatureCode, NpgsqlDbType.Text);
        await Writer.WriteAsync(dependencyFeatureCode, NpgsqlDbType.Text);
    }
}
