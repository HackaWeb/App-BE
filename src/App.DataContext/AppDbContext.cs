using App.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace App.DataContext;

public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public const string DefaultSchemaName = "dbo";

    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Sample> Samples { get; set; }
    public DbSet<ChildSample> ChildSamples { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Sample>()
            .HasMany(s => s.ChildSamples)
            .WithOne(c => c.Sample)
            .HasForeignKey(c => c.SampleId);

        builder.Entity<User>()
            .HasMany(s => s.Samples)
            .WithOne(s => s.User)
            .HasForeignKey(s => s.UserId);

        base.OnModelCreating(builder);
    }
}
