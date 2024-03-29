using Npgsql;
using NpgsqlTypes;
using SpecBox.Domain.Model.Enums;

namespace SpecBox.Domain.BulkCopy;

public class BulkWriterAssertion : BulkWriter
{
    private const string COMMAND =
        "COPY \"ExportAssertion\" (\"ExportId\", \"FeatureCode\",\"GroupTitle\",\"Title\",\"Description\", \"AutomationState\") FROM STDIN (FORMAT BINARY)";

    public BulkWriterAssertion(NpgsqlConnection connection) : base(COMMAND, connection)
    {
    }

    public async Task AddAssertion(
        Guid exportId,
        string featureCode,
        string groupTitle,
        string title,
        string? description,
        AutomationState automationState)
    {
        await Writer.StartRowAsync();
        await Writer.WriteAsync(exportId, NpgsqlDbType.Uuid);
        await Writer.WriteAsync(featureCode, NpgsqlDbType.Text);
        await Writer.WriteAsync(groupTitle, NpgsqlDbType.Text);
        await Writer.WriteAsync(title, NpgsqlDbType.Text);
        await Writer.WriteAsync(description, NpgsqlDbType.Text);
        await Writer.WriteAsync(Convert.ToInt32(automationState), NpgsqlDbType.Integer);
    }
}
