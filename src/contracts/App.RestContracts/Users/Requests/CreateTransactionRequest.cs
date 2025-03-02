namespace App.RestContracts.Users.Requests
{
    public class CreateTransactionRequest
    {
        public DateTime CreatedAt { get; set; }
        public decimal Amount { get; set; }
    }
}
