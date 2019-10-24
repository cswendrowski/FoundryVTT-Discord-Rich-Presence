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
        readonly Timer _activityStatusTimeoutTimer;
        readonly Timer _discordTimer;
        readonly TimeSpan TimeToWaitBeforeRemovingRichPresence = TimeSpan.FromMinutes(1);
        readonly TimeSpan DiscordRefreshRate = TimeSpan.FromMilliseconds(1000 / 60);

        public PlayerStatusController()
        {
            _discord = CreateDiscord();
            _activityStatusTimeoutTimer = new Timer(OnActivityStatusTimerElapsed, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

            _discordTimer = new Timer(OnDiscordUpdate, null, DiscordRefreshRate, DiscordRefreshRate);
        }

        private Discord CreateDiscord()
        {
            Console.WriteLine("Creating new Discord instance");
            return new Discord(long.Parse("635971834499563530"), (ulong)CreateFlags.NoRequireDiscord);
        }

        private void OnDiscordUpdate(object state)
        {
            if (_discord != null)
            {
                try
                {
                    lock (_discord)
                    {
                        _discord.RunCallbacks();
                    }
                }
                catch { }
            }
        }

        private void OnActivityStatusTimerElapsed(object state)
        {
            if (_discord == null) return;

            lock (_discord)
            {
                var activityManager = _discord.GetActivityManager();

                activityManager.ClearActivity(result =>
                {
                    Console.WriteLine("Clear activity result: " + result);
                });


                _activityStatusTimeoutTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                _discordTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

                Console.WriteLine("Disposing of current Discord connection to clear Playing status");

                _discord.Dispose();
                _discord = null;

                Console.WriteLine("Done disposing!");
            }
        }

        [Route("")]
        [HttpPost]
        public void PutPlayerStatus([FromBody] PlayerStatus playerStatus)
        {
            lock (_discord)
            {
                if (_discord == null)
                {
                    _discord = CreateDiscord();
                    _discordTimer.Change(DiscordRefreshRate, DiscordRefreshRate);
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
                    Console.WriteLine("Set activity result: " + result);
                });
            }

            _activityStatusTimeoutTimer.Change(TimeToWaitBeforeRemovingRichPresence, Timeout.InfiniteTimeSpan);
        }

        [Route("leave")]
        [HttpPost]
        public void LeaveGame()
        {
            Console.WriteLine("Foundry module has initiated a leave");
            OnActivityStatusTimerElapsed(null);
        }
    }
}
