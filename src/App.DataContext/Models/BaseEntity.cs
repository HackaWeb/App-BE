using System.ComponentModel.DataAnnotations;

namespace App.DataContext.Models;

internal class BaseEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
}