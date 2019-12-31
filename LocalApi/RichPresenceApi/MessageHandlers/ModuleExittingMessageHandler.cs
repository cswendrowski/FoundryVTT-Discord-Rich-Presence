using DiscordSdk;
using RichPresenceApi.Controllers;
using RichPresenceApi.Models;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace RichPresenceApi.MessageHandlers
{
    public class ModuleExittingMessageHandler : AbstractMessageHandler<ModuleExittingMessage>
    {
        public override RichPresenceMessageTypes MessageType => RichPresenceMessageTypes.ModuleExitting;

        protected override async Task OnMessage(ModuleExittingMessage message)
        {
            Console.WriteLine("Foundry module has initiated a leave");
            DiscordManager.RemoveStatus();

            if (CurrentLobby.HasValue)
            {
                DiscordManager.GetDiscord().GetLobbyManager().DisconnectLobby(CurrentLobby.Value.Id, (Result result) => { });
                CurrentLobby = null;
            }

            await StatusWebsocket.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Foundry module initiated a shutdown", CancellationToken.None);

            if (message.ShouldExit)
            {
                Console.WriteLine("Foundry asked the API to exit, exitting..");
                Environment.Exit(0);
            }
        }
    }
}
