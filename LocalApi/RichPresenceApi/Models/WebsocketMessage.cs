using Newtonsoft.Json;

namespace RichPresenceApi.Models
{
    public class WebsocketMessage
    {
        public RichPresenceMessageTypes Type { get; set; }

        public string Payload { get; set; }

        public T As<T>()
        {
            return JsonConvert.DeserializeObject<T>(Payload);
        }

        public void From<T>(T data)
        {
            Payload = JsonConvert.SerializeObject(data);
        }
    }

    public enum RichPresenceMessageTypes
    {
        Unset = 0,

        // One time messages
        ModuleLaunched = 1,
        ModuleExitting = 2,

        // Once per User
        DiscordProfileInfo = 3,

        // Continuous updates
        GameStatus = 4,
        VoiceStatus = 5,
        VoiceLobby = 6,
        VoiceOptions = 7,
        Speaking = 8,

    }
}
