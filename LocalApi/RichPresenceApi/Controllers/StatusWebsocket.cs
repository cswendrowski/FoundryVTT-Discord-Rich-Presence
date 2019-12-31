using Newtonsoft.Json;
using RichPresenceApi.MessageHandlers;
using RichPresenceApi.Models;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace RichPresenceApi.Controllers
{
    public static class StatusWebsocket
    {
        public static WebSocket WebSocket { get; set; }

        public static async Task OnMessage(string data)
        {
            var message = JsonConvert.DeserializeObject<WebsocketMessage>(data);

            switch (message.Type)
            {
                case RichPresenceMessageTypes.ModuleLaunched: await new ModuleLaunchedMessageHandler().HandleMessage(message); return;
                case RichPresenceMessageTypes.ModuleExitting: await new ModuleExittingMessageHandler().HandleMessage(message); return;

                case RichPresenceMessageTypes.GameStatus: await new GameStatusMessageHandler().HandleMessage(message); return;
                case RichPresenceMessageTypes.VoiceLobby: await new VoiceLobbyMessageHandler().HandleMessage(message); return;
                case RichPresenceMessageTypes.VoiceStatus: await new VoiceStatusMessageHandler().HandleMessage(message); return;
                case RichPresenceMessageTypes.VoiceOptions: await new VoiceOptionsMessageHandler().HandleMessage(message); return;

                default: return;
            }
        }

    }
}
