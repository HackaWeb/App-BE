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

    public DbSet<Notification> Notifications { get; set; }
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

        builder.Entity<User>()
            .HasMany(u => u.Notifications)
            .WithOne(n => n.User)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserTag>()
            .HasOne(ut => ut.User)
            .WithMany(u => u.UserTags)
            .HasForeignKey(ut => ut.UserId);

        builder.Entity<UserTag>()
            .HasOne(ut => ut.Tag)
            .WithMany(t => t.UserTags)
            .HasForeignKey(ut => ut.TagId);

        builder.Entity<Notification>()
            .HasOne(n => n.Sender)
            .WithMany()
            .HasForeignKey(n => n.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        base.OnModelCreating(builder);
    }
}
