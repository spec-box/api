﻿using Microsoft.EntityFrameworkCore;
using SpecBox.Domain.Model;
using Attribute = SpecBox.Domain.Model.Attribute;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Feature>()
            .HasMany(e => e.Attributes)
            .WithMany(e => e.Features)
            .UsingEntity<FeatureAttributeValue>(
                x => x.HasOne<AttributeValue>().WithMany().HasForeignKey(x => x.AttributeValueId),
                x => x.HasOne<Feature>().WithMany().HasForeignKey(x => x.FeatureId)
            );

        modelBuilder.Entity<TreeNode>()
            .HasMany(e => e.Features)
            .WithMany(e => e.TreeNodes)
            .UsingEntity<TreeNodeFeature>(
                x => x.HasOne<Feature>().WithMany().HasForeignKey(x => x.FeatureId),
                x => x.HasOne<TreeNode>().WithMany().HasForeignKey(x => x.TreeNodeId)
            );
    }
}
