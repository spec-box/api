using Npgsql;

namespace SpecBox.Domain.BulkCopy;

public static class BulkWriterExtensions
{
    public static BulkWriterFeature CreateFeatureWriter(this NpgsqlConnection connection)
    {
        return new BulkWriterFeature(connection);
    }

    public static BulkWriterAssertion CreateAssertionWriter(this NpgsqlConnection connection)
    {
        return new BulkWriterAssertion(connection);
    }

    public static BulkWriterFeatureAttribute CreateFeatureAttributeWriter(this NpgsqlConnection connection)
    {
        return new BulkWriterFeatureAttribute(connection);
    }
}
