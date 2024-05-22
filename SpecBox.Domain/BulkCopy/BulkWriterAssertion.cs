using Npgsql;
using NpgsqlTypes;
using SpecBox.Domain.Model.Enums;

namespace SpecBox.Domain.BulkCopy;

public class BulkWriterAssertion : BulkWriter
{
    private const string COMMAND =
        "COPY \"ExportAssertion\" (\"ExportId\", \"FeatureCode\",\"GroupTitle\",\"GroupSortOrder\",\"Title\",\"Description\",\"DetailsUrl\",\"SortOrder\",\"AutomationState\") FROM STDIN (FORMAT BINARY)";

    public BulkWriterAssertion(NpgsqlConnection connection) : base(COMMAND, connection)
    {
    }

    public async Task AddAssertion(
        Guid exportId,
        string featureCode,
        string groupTitle,
        int? groupSortOrder,
        string title,
        string? description,
        string? detailsUrl,
        int? sortOrder,
        AutomationState automationState)
    {
        await Writer.StartRowAsync();
        await Writer.WriteAsync(exportId, NpgsqlDbType.Uuid);
        await Writer.WriteAsync(featureCode, NpgsqlDbType.Text);
        await Writer.WriteAsync(groupTitle, NpgsqlDbType.Text);
        await WriteNullableInt32(groupSortOrder);
        await Writer.WriteAsync(title, NpgsqlDbType.Text);
        await Writer.WriteAsync(description, NpgsqlDbType.Text);
        await Writer.WriteAsync(detailsUrl, NpgsqlDbType.Text);
        await WriteNullableInt32(sortOrder);
        await Writer.WriteAsync(Convert.ToInt32(automationState), NpgsqlDbType.Integer);
    }
}
