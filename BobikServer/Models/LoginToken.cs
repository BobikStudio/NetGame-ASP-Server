namespace BobikServer.Models
{
    public class LoginToken
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public AccountProfile AccountProfile { get; set; }
        public string Token { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
