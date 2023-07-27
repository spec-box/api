using Microsoft.EntityFrameworkCore;
using tms.Domain.Model;

namespace tms.Domain;

public class TmsDbContext : DbContext
{
    public TmsDbContext(DbContextOptions<TmsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<Entity> Entities { get; set; } = null!;
}
