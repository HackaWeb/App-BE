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
}
