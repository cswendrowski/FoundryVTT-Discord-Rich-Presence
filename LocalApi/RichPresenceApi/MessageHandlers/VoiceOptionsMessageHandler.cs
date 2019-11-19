using DiscordSdk;
using RichPresenceApi.Models;
using System;
using System.Threading.Tasks;

namespace RichPresenceApi.MessageHandlers
{
    public class VoiceOptionsMessageHandler : AbstractMessageHandler<VoiceOptionsMessage>
    {
        public override RichPresenceMessageTypes MessageType => RichPresenceMessageTypes.VoiceOptions;

        protected override async Task OnMessage(VoiceOptionsMessage message)
        {
            DiscordManager.GetDiscord().GetOverlayManager().OpenVoiceSettings((result) =>
            {
                if (result == Result.Ok)
                {
                    Console.WriteLine("Voice options have now been opened");
                }
            });
        }
    }
}
