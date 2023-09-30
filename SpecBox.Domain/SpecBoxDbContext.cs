using System.Data;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using SpecBox.Domain.Model;
using Attribute = SpecBox.Domain.Model.Attribute;
using Npgsql;
using SpecBox.Domain.BulkCopy;

namespace SpecBox.Domain;

public class SpecBoxDbContext : DbContext
{
    public SpecBoxDbContext(DbContextOptions<SpecBoxDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects { get; set; } = null!;

    public DbSet<Feature> Features { get; set; } = null!;

    public DbSet<AssertionGroup> AssertionGroups { get; set; } = null!;

    public DbSet<Assertion> Assertions { get; set; } = null!;

    public DbSet<Attribute> Attributes { get; set; } = null!;

    public DbSet<AttributeValue> AttributeValues { get; set; } = null!;

    public DbSet<TreeNode> TreeNodes { get; set; } = null!;

    public DbSet<Tree> Trees { get; set; } = null!;

    public DbSet<AttributeGroupOrder> AttributeGroupOrders { get; set; } = null!;

    public DbSet<AutotestsStatRecord> AutotestsStat { get; set; } = null!;

    public DbSet<AssertionsStatRecord> AssertionsStat { get; set; } = null!;

    public DbSet<Export> Exports { get; set; } = null!;

    // bulk copy
    public async Task<BulkWriterFeature> CreateFeatureWriter()
    {
        var connection = await GetConnection();

        return new BulkWriterFeature(connection);
    }

    public async Task BuildTree(Guid projectId) => await ExecuteSQL("CALL \"BuildTree\"($1)", projectId);
    public async Task MergeExportedData(Guid exportId) => await ExecuteSQL("CALL \"MergeExportedData\"($1)", exportId);

    private async Task<NpgsqlConnection> GetConnection()
    {
        var connection = Database.GetDbConnection() as NpgsqlConnection;

        Debug.Assert(connection != null, nameof(connection) + " != null");

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }
        
        return connection;
    }

    private async Task ExecuteSQL(string commandText, params object[] args) {
        var connection = await GetConnection();
        
        await using var command = connection.CreateCommand();
        command.CommandText = commandText;

        foreach (var paramValue in args)
        {
            var param = command.CreateParameter();
            param.Value = paramValue;
            command.Parameters.Add(param);
        }

        await command.ExecuteNonQueryAsync();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Feature>()
            .HasMany(e => e.Attributes)
            .WithMany(e => e.Features)
            .UsingEntity<FeatureAttributeValue>(
                x => x.HasOne<AttributeValue>().WithMany().HasForeignKey(x => x.AttributeValueId),
                x => x.HasOne<Feature>().WithMany().HasForeignKey(x => x.FeatureId)
            );
    }
}
