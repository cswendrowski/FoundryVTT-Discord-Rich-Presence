using DiscordSdk;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Cryptography;
using System.Text;
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

        public static byte[] GetHash(string inputString)
        {
            var algorithm = SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        private string ToUf8(string strFrom)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(strFrom);
            return Encoding.UTF8.GetString(bytes);
        }

        [Route("")]
        [HttpPost]
        public void PutPlayerStatus([FromBody] PlayerStatus playerStatus)
        {
            var activity = new Activity()
            {
                Details = ToUf8(string.IsNullOrEmpty(playerStatus.ActorName) ? $"Playing {playerStatus.SystemName}" : $"Playing as {playerStatus.ActorName}"),
                State = $"Exploring {playerStatus.SceneName}",
                Party = new ActivityParty { Id = playerStatus.WorldUniqueId, Size = new PartySize { CurrentSize = playerStatus.CurrentPlayerCount, MaxSize = playerStatus.MaxPlayerCount } },
                Assets = new ActivityAssets
                {
                    LargeImage = "d20",
                    LargeText = "D20"
                },
                Instance = false,
                Secrets = new ActivitySecrets { Join = $"{GetHash(playerStatus.WorldUniqueId)}" }
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
