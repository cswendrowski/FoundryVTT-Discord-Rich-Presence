using DiscordSdk;
using Microsoft.AspNetCore.Mvc;
using System;
using TestApi.Models;

namespace TestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerStatusController : ControllerBase
    {
        public PlayerStatusController()
        {
            DiscordManager.CreateDiscord();
        }

        [Route("")]
        [HttpPost]
        public void PutPlayerStatus([FromBody] PlayerStatus playerStatus)
        {
            var activity = new Activity()
            {
                Details = string.IsNullOrEmpty(playerStatus.ActorName) ? $"Playing {playerStatus.SystemName}" : $"Playing as {playerStatus.ActorName}",
                State = $"Exploring {playerStatus.SceneName}",
                Party = new ActivityParty { Id = Guid.NewGuid().ToString(), Size = new PartySize { CurrentSize = playerStatus.CurrentPlayerCount, MaxSize = playerStatus.MaxPlayerCount } },
                Assets = new ActivityAssets
                {
                    LargeImage = "d20",
                    LargeText = "D20"
                },
                Instance = false,
                Secrets = new ActivitySecrets { Match = playerStatus.WorldUniqueId, Join = $"{playerStatus.FoundryUrl}/join" }
            };

            if (playerStatus.IsGm)
            {
                activity.Details = "GMing";
                activity.State = $"Playing {playerStatus.SystemName}";
            }

            DiscordManager.SetActivity(activity, playerStatus);
        }

        [Route("leave")]
        [HttpPost]
        public void LeaveGame()
        {
            Console.WriteLine("Foundry module has initiated a leave");
            DiscordManager.RemoveStatus();
        }
    }
}
