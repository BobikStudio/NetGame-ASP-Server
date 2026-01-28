namespace BobikServer.Models
{
    public class AccountProfile
    {
        public int Id { get; set; }
        public string IdentityId { get; set; }
        public List<GameProfile> GameProfiles { get; set; }
        public List<LoginToken> LoginTokens { get; set; }
        public List<ConnectGameserverToken> ConnectGameserverTokens { get; set; }
    }
}
