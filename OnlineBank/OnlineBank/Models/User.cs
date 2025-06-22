namespace OnlineBank.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string GovId { get; set; }
        public string BankAccountNumber { get; set; }
        public decimal Balance { get; set; }
        public string Role {  get; set; }
        public string PasswordHash { get; set; }
    }
}
