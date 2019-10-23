using DiscordSdk;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using TestApi.Models;

namespace TestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerStatusController : ControllerBase
    {
        Discord _discord;
        readonly Timer _timer;
        readonly TimeSpan TimeToWaitBeforeRemovingRichPresence = TimeSpan.FromSeconds(20);

        public PlayerStatusController()
        {
            _discord = CreateDiscord();
            _timer = new Timer(OnActivityStatusTimerElapsed, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

            var discordTimer = new Timer(OnDiscordUpdate, null, TimeSpan.FromMilliseconds(1000 / 60), TimeSpan.FromMilliseconds(1000 / 60));
        }

        private Discord CreateDiscord()
        {
            return new Discord(long.Parse("635971834499563530"), (ulong)CreateFlags.Default);
        }

        private void OnDiscordUpdate(object state)
        {
            if (_discord != null)
            {
                _discord.RunCallbacks();
            }
        }

        private void OnActivityStatusTimerElapsed(object state)
        {
            var activityManager = _discord.GetActivityManager();

            activityManager.ClearActivity(result => {
                Console.WriteLine(result);
            });

            _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

            _discord.Dispose();
            _discord = null;
        }

        [Route("")]
        [HttpPost]
        public void PutPlayerStatus([FromBody] PlayerStatus playerStatus)
        {
            if (_discord == null)
            {
                _discord = CreateDiscord();
            }

            var activity = new Activity()
            {
                Details = $"Playing as {playerStatus.ActorName}",
                State = $"Exploring {playerStatus.SceneName}",
                Party = new ActivityParty { Id = Guid.NewGuid().ToString(), Size = new PartySize { CurrentSize = playerStatus.CurrentPlayerCount, MaxSize = playerStatus.MaxPlayerCount } },
                Assets = new ActivityAssets
                {
                    LargeImage = "d20",
                    LargeText = "D20"
                },
                Instance = true,
                Secrets = new ActivitySecrets { Match = playerStatus.WorldUniqueId, Join = $"{playerStatus.FoundryUrl}/join" }
            };

            if (playerStatus.IsGm)
            {
                activity.Details = "GMing";
                activity.State = "";
            }

            _discord.GetActivityManager().UpdateActivity(activity, result =>
            {
                _timer.Change(TimeToWaitBeforeRemovingRichPresence, Timeout.InfiniteTimeSpan);
                Console.WriteLine(result.ToString());
            });
        }

        [Route("leave")]
        [HttpPost]
        public void LeaveGame()
        {
            OnActivityStatusTimerElapsed(null);
        }
    }
}
