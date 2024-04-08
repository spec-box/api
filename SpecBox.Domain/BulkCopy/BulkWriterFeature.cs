using Npgsql;
using NpgsqlTypes;
using SpecBox.Domain.Model.Enums;

namespace SpecBox.Domain.BulkCopy;

public class BulkWriterFeature : BulkWriter
{
    private const string COMMAND =
        "COPY \"ExportFeature\" (\"ExportId\", \"Code\", \"Title\",\"Description\", \"FeatureType\", \"FilePath\") FROM STDIN (FORMAT BINARY)";

    public BulkWriterFeature(NpgsqlConnection connection) : base(COMMAND, connection)
    {
    }

    private int? ToNullableInt32(FeatureType? featureType)
    {
        if (featureType.HasValue)
        {
            return Convert.ToInt32(featureType.Value);
        }

        return null;
    }

    public async Task AddFeature(
        Guid exportId,
        string code,
        string title,
        string? description,
        FeatureType? featureType,
        string? filePath)
    {
        await Writer.StartRowAsync();
        await Writer.WriteAsync(exportId, NpgsqlDbType.Uuid);
        await Writer.WriteAsync(code, NpgsqlDbType.Text);
        await Writer.WriteAsync(title, NpgsqlDbType.Text);
        await Writer.WriteAsync(description, NpgsqlDbType.Text);
        await WriteNullableInt32(ToNullableInt32(featureType));
        await Writer.WriteAsync(filePath, NpgsqlDbType.Text);
    }
}
