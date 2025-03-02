using App.Domain.Enums;

namespace App.Domain.Models
{
    public class Transaction : BaseModel
    {
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public DateTime TransactionDate { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
    }
}
