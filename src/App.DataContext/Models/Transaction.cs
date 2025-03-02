using App.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace App.DataContext.Models;

internal class Transaction : BaseEntity
{
    [Required]
    public TransactionType Type { get; set; }

    [Required]
    public decimal Amount { get; set; }

    [Required]
    public decimal Balance { get; set; }

    [Required]
    public DateTime TransactionDate { get; set; }

    public Guid UserId { get; set; }
    public virtual User User { get; set; }
}