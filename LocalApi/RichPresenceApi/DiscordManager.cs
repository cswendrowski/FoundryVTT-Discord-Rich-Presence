using DiscordSdk;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using TestApi.Models;

namespace TestApi
{
    public static class DiscordManager
    {
        static string WhoAmI = Guid.NewGuid().ToString();
        static Discord _discord;
        static Timer _discordTimer;

        static TimeSpan DiscordRefreshRate = TimeSpan.FromMilliseconds(1000 / 60);
        static readonly TimeSpan TimeToWaitBeforeRemovingRichPresence = TimeSpan.FromMinutes(1);

        public static bool IsCurrentlyDisposing { get; set; } = false;
        public static DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
        public static bool RecentlyGotUpdate { get; set; } = false;

        static DiscordManager()
        {
            Console.WriteLine("Created DiscordManager " + WhoAmI);
            _discordTimer = new Timer(OnDiscordUpdate, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            CreateDiscord();

            var command = "cmd.exe /c start foundryvtt-richpresence://run";
            _discord.GetActivityManager().RegisterCommand(command);
            Console.WriteLine("Registered command " + command);
        }

        public static void CreateDiscord()
        {
            if (_discord != null) return;

            Console.WriteLine(WhoAmI + " - Creating new Discord instance");
            _discord = new Discord(long.Parse("635971834499563530"), (ulong)CreateFlags.Default);

            _discord.GetActivityManager().OnActivityJoin += HandleActivityJoin;

            _discordTimer.Change(DiscordRefreshRate, DiscordRefreshRate);
        }

        private static void HandleActivityJoin(string secret)
        {
            Console.WriteLine("Handling Activity Join with secret " + secret);
            Process.Start(secret);
        }

        public static Discord GetDiscord()
        {
            if (_discord == null)
            {
                Console.WriteLine("Need a Discord instance");
                CreateDiscord();
            }
            return _discord;
        }

        internal static void SetActivity(DiscordSdk.Activity activity)
        {
            if (IsCurrentlyDisposing) return;

            var guid = Guid.NewGuid().ToString();

            var activityManager = GetDiscord().GetActivityManager();

            activityManager.UpdateActivity(activity, result =>
            {
                Console.WriteLine(guid + " - Set activity result: " + result);
            });

            RecentlyGotUpdate = true;
            LastUpdated = DateTimeOffset.UtcNow;
            Console.WriteLine("LastUpdated: " + LastUpdated.ToString("T"));
        }

        public static void DisableDiscordUpdates()
        {
            _discordTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        public static void Dispose()
        {
            if (_discord == null)
            {
                return;
            };

            IsCurrentlyDisposing = true;

            _discord.GetActivityManager().ClearActivity(result =>
            {
                Console.WriteLine("Clear activity: " + result);
            });

            Console.WriteLine("Disposing of current Discord connection to clear Playing status");

            _discord.Dispose();
            _discord = null;

            Console.WriteLine("Done disposing!");
            IsCurrentlyDisposing = false;
        }

        private static void OnDiscordUpdate(object state)
        {
            if (RecentlyGotUpdate && DateTimeOffset.UtcNow.Subtract(LastUpdated) >= TimeToWaitBeforeRemovingRichPresence)
            {
                Console.WriteLine("Expired! LastUpdated: " + LastUpdated.ToString("T"));
                OnActivityStatusTimerElapsed();
            }

            if (_discord != null)
            {
                try
                {
                    //Console.WriteLine("Callback: " + WhoAmI);
                    if (IsCurrentlyDisposing) return;
                    _discord.RunCallbacks();
                }
                catch (SEHException) { }
                catch { }
            }
        }

        public static void OnActivityStatusTimerElapsed()
        {
            Console.WriteLine("Haven't heard from the module in awhile, clearing status");

            RemoveStatus();
        }

        public static void RemoveStatus()
        {
            RecentlyGotUpdate = false;
            DisableDiscordUpdates();
            Dispose();
        }
    }
}
