using DiscordSdk;
using RichPresenceApi.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RichPresenceApi.MessageHandlers
{
    public class ModuleLaunchedMessageHandler : AbstractMessageHandler<ModuleLaunchedMessage>
    {
        public override RichPresenceMessageTypes MessageType => RichPresenceMessageTypes.ModuleLaunched;

        protected override async Task OnMessage(ModuleLaunchedMessage message)
        {
            var userManager = DiscordManager.GetDiscord().GetUserManager();

            userManager.OnCurrentUserUpdate += async () =>
            {
                var currentUser = userManager.GetCurrentUser();

                Console.WriteLine("User fetched: {0} ({1})", currentUser.Username, currentUser.Id);

                // Request users's avatar data.
                // This can only be done after a user is successfully fetched.
                var imageManager = DiscordManager.GetDiscord().GetImageManager();
                imageManager.Fetch(ImageHandle.User(currentUser.Id), async (result, handle) =>
                {
                    if (result == Result.Ok)
                    {
                        Console.WriteLine("Fetched Image, processing..");
                        // You can also use GetTexture2D within Unity.
                        // These return raw RGBA.
                        var data = imageManager.GetData(handle);

                        var rgbaList = new List<Rgba32>();

                        for (int x = 0; x < data.Length; x += 4)
                        {
                            rgbaList.Add(new Rgba32(data[x], data[x + 1], data[x + 2], data[x + 3]));
                        }

                        var image = Image.LoadPixelData<Rgba32>(data, 128, 128);

                        var base64 = image.ToBase64String(PngFormat.Instance);
#if DEBUG
                        Console.WriteLine(base64);
#endif
                        var discordProfileInfo = new DiscordProfileInfoMessage
                        {
                            AvatarBase64 = base64,
                            DiscordId = currentUser.Id
                        };

                        Console.WriteLine("Sent avatar!");

                        var msg = new WebsocketMessage()
                        {
                            Type = RichPresenceMessageTypes.DiscordProfileInfo
                        };
                        msg.From(discordProfileInfo);

                        await Send(msg);
                    }
                    else
                    {
                        Console.WriteLine("Image error {0}", handle.Id);
                    }
                });

                var lobbyManager = DiscordManager.GetDiscord().GetLobbyManager();
                var voiceManager = DiscordManager.GetDiscord().GetVoiceManager();

                if (CurrentLobby.HasValue)
                {
                    var msg = new WebsocketMessage()
                    {
                        Type = RichPresenceMessageTypes.VoiceStatus
                    };
                    msg.From(new VoiceStatusMessage { IsConnected = true, IsDeafened = voiceManager.IsSelfDeaf(), IsMuted = IsSelfMuted(voiceManager) });
                    await Send(msg);
                }

                lobbyManager.OnSpeaking += HandleSpeaking;

            };

            void HandleSpeaking(long lobbyId, long userId, bool speaking)
            {
                var msg = new WebsocketMessage()
                {
                    Type = RichPresenceMessageTypes.Speaking
                };
                msg.From(new SpeakingMessage { DiscordId = userId, IsSpeaking = speaking });
                Send(msg).GetAwaiter().GetResult();
            }
        }
    }
}
