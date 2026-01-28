namespace BobikServer.Models
{
    public class GameProfile
    {
        public int Id { get; set; }
        public int AccountProfileId { get; set; }
        public AccountProfile AccountProfile { get; set; }
        public string Nickname { get; set; }

        public int Coins { get; set; }
    }
}
