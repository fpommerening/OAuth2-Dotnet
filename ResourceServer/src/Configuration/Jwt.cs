namespace FP.OAuth.ResourceServer.Configuration
{
    public class Jwt
    {
        public string[] Audiences { get; set; } 

        public string Authority { get; set; }

        public string SigningKey { get; set; }
    }
}
