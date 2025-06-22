namespace OnlineBank.DTOs
{
    public class SendMoneyDto
    {
        public string RecipientFullName { get; set; }
        public string RecipientBankAccountNumber { get; set;}
        public string Description { get; set; }
        public decimal Amount { get; set; }
    }
}
