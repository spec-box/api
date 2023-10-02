using Npgsql;
using NpgsqlTypes;

namespace SpecBox.Domain.BulkCopy;

public class BulkWriterFeatureAttribute : BulkWriter
{
    private const string COMMAND =
        "COPY \"ExportFeatureAttribute\" (\"ExportId\", \"FeatureCode\",\"AttributeCode\",\"AttributeValueCode\") FROM STDIN (FORMAT BINARY)";

    public BulkWriterFeatureAttribute(NpgsqlConnection connection) : base(COMMAND, connection)
    {
    }

    public async Task AddFeatureAttribute(
        Guid exportId,
        string featureCode,
        string attributeCode,
        string attributeValueCode)
    {
        await Writer.StartRowAsync();
        await Writer.WriteAsync(exportId, NpgsqlDbType.Uuid);
        await Writer.WriteAsync(featureCode, NpgsqlDbType.Text);
        await Writer.WriteAsync(attributeCode, NpgsqlDbType.Text);
        await Writer.WriteAsync(attributeValueCode, NpgsqlDbType.Text);
    }
}
