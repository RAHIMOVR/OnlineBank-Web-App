namespace OnlineBank.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public string SenderBankAccountNumber { get; set; }
        public string RecipientBankAccountNumber { get; set; }
        public string RecipientFullName { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
