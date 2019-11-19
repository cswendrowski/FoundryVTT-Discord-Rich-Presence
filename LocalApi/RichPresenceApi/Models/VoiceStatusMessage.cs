namespace RichPresenceApi.Models
{
    public class VoiceStatusMessage
    {
        public bool IsConnected { get; set; }

        public bool? IsMuted { get; set; }

        public bool? IsDeafened { get; set; }
    }
}
