using RichPresenceApi.Models;
using System;
using System.Threading.Tasks;

namespace RichPresenceApi.MessageHandlers
{
    public class VoiceStatusMessageHandler : AbstractMessageHandler<VoiceStatusMessage>
    {
        public override RichPresenceMessageTypes MessageType => RichPresenceMessageTypes.VoiceStatus;

        protected override async Task OnMessage(VoiceStatusMessage voiceStatusMessage)
        {
            if (voiceStatusMessage.IsMuted.HasValue)
            {
                await HandleMute();
            }
            if (voiceStatusMessage.IsDeafened.HasValue)
            {
                await HandleDeafen();
            }
        }


        private async Task HandleMute()
        {
            var voiceManager = DiscordManager.GetDiscord().GetVoiceManager();
            voiceManager.SetSelfMute(!voiceManager.IsSelfMute());

            var isMuted = voiceManager.IsSelfMute();
            Console.WriteLine("Mute changed to " + isMuted);

            var msg = new WebsocketMessage()
            {
                Type = RichPresenceMessageTypes.VoiceStatus
            };
            msg.From(new VoiceStatusMessage { IsConnected = true, IsMuted = IsSelfMuted(voiceManager), IsDeafened = voiceManager.IsSelfDeaf() });
            await Send(msg);
        }

        private async Task HandleDeafen()
        {
            var voiceManager = DiscordManager.GetDiscord().GetVoiceManager();
            voiceManager.SetSelfDeaf(!voiceManager.IsSelfDeaf());

            var isDeafened = voiceManager.IsSelfDeaf();
            Console.WriteLine("Deafened changed to " + isDeafened);

            var msg = new WebsocketMessage()
            {
                Type = RichPresenceMessageTypes.VoiceStatus
            };
            msg.From(new VoiceStatusMessage { IsConnected = true, IsMuted = IsSelfMuted(voiceManager), IsDeafened = voiceManager.IsSelfDeaf() });
            await Send(msg);
        }
    }
}
