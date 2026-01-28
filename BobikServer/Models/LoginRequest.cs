namespace BobikServer.Models
{
    public class LoginRequest
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string LoginToken { get; set; }     
        public bool CreateToken { get; set; } 
    }
}
