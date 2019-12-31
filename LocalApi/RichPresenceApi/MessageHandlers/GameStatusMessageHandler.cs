using DiscordSdk;
using RichPresenceApi.Models;
using System.Threading.Tasks;
using TestApi.Models;

namespace RichPresenceApi.MessageHandlers
{
    public class GameStatusMessageHandler : AbstractMessageHandler<GameStatusMessage>
    {
        public override RichPresenceMessageTypes MessageType => RichPresenceMessageTypes.GameStatus;

        protected override async Task OnMessage(GameStatusMessage playerStatus)
        {
            var activity = new Activity()
            {
                Details = playerStatus.Details,
                State = playerStatus.State,
                Party = new ActivityParty { Id = playerStatus.WorldUniqueId, Size = new PartySize { CurrentSize = playerStatus.CurrentPlayerCount, MaxSize = playerStatus.MaxPlayerCount } },
                Assets = new ActivityAssets
                {
                    LargeImage = "d20",
                    LargeText = "D20"
                },
                Instance = false,
                Secrets = new ActivitySecrets { Join = $"{playerStatus.FoundryUrl}/join" }
            };

            DiscordManager.SetActivity(activity);
        }
    }
}
