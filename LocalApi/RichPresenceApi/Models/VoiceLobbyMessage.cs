namespace RichPresenceApi.Models
{
    public class VoiceLobbyMessage
    {
        public bool ShouldConnect { get; set; }

        public int VoicePartySize { get; set; }

        public string WorldUniqueIdentifier { get; set; } = "";
    }
}
