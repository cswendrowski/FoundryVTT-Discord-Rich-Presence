using DiscordSdk;
using Newtonsoft.Json;
using RichPresenceApi.Controllers;
using RichPresenceApi.Models;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RichPresenceApi.MessageHandlers
{
    public abstract class AbstractMessageHandler<T>
    {
        protected static Lobby? CurrentLobby = null;

        public abstract RichPresenceMessageTypes MessageType { get; }

        public async Task HandleMessage(WebsocketMessage message)
        {
            var msg = JsonConvert.DeserializeObject<T>(message.Payload);
            await OnMessage(msg);
        }

        protected abstract Task OnMessage(T message);

        protected async Task Send<Message>(Message message) where Message : WebsocketMessage
        {
            if (StatusWebsocket.WebSocket != null)
            {
                var json = JsonConvert.SerializeObject(message);
                var data = Encoding.UTF8.GetBytes(json);
                var buffer = new ArraySegment<byte>(data);
                await StatusWebsocket.WebSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        protected bool IsSelfMuted(VoiceManager voiceManager)
        {
            return voiceManager.IsSelfMute() || voiceManager.IsSelfDeaf();
        }
    }
}
