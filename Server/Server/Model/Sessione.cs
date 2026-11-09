namespace Server.Model
{
    public class Sessione
    {
        public string SessionId { get; set; } = "";

        public byte[] Chiave { get; set; } = Array.Empty<byte>();
    }
}
