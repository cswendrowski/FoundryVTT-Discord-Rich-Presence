using DiscordSdk;
using RichPresenceApi.Models;
using System;
using System.Threading.Tasks;

namespace RichPresenceApi.MessageHandlers
{
    public class VoiceLobbyMessageHandler : AbstractMessageHandler<VoiceLobbyMessage>
    {
        public override RichPresenceMessageTypes MessageType => RichPresenceMessageTypes.VoiceLobby;

        protected override async Task OnMessage(VoiceLobbyMessage message)
        {
            if (message.ShouldConnect)
            {
                await HandleClickedConnect(message);
            }
            else
            {
                await HandleClickedDisconnect();
            }
        }

        private async Task HandleClickedConnect(VoiceLobbyMessage voiceLobbyMessage)
        {
            var lobbyManager = DiscordManager.GetDiscord().GetLobbyManager();

            // Search lobbies.
            var query = lobbyManager.GetSearchQuery();
            // Filter by a metadata value.
            query.Filter("metadata.FoundryRemoteIp", LobbySearchComparison.Equal, LobbySearchCast.String, voiceLobbyMessage.WorldUniqueIdentifier);
            // Only return 1 result max.
            query.Limit(1);

            Console.WriteLine("Searching for existing Lobbies with FoundryRemoteIp of " + voiceLobbyMessage.WorldUniqueIdentifier);

            lobbyManager.Search(query, (_) =>
            {
                Console.WriteLine("Search returned {0} lobbies", lobbyManager.LobbyCount());
                if (lobbyManager.LobbyCount() == 1)
                {
                    CurrentLobby = lobbyManager.GetLobby(lobbyManager.GetLobbyId(0));
                    Console.WriteLine("Sirst lobby secret: {0}", CurrentLobby.Value.Secret);

                    lobbyManager.ConnectLobby(CurrentLobby.Value.Id, CurrentLobby.Value.Secret, (Result result, ref Lobby connectedLobby) =>
                    {
                        CurrentLobby = connectedLobby;
                        Console.WriteLine("Connected to Lobby " + CurrentLobby.Value.Id + " ? " + result);

                        // Connect to voice chat.
                        lobbyManager.ConnectVoice(CurrentLobby.Value.Id, async (_) =>
                        {
                            Console.WriteLine("Connected to voice chat!");
                            var voiceManager = DiscordManager.GetDiscord().GetVoiceManager();
                            var msg = new WebsocketMessage()
                            {
                                Type = RichPresenceMessageTypes.VoiceStatus
                            };
                            msg.From(new VoiceStatusMessage { IsConnected = true, IsMuted = voiceManager.IsSelfMute(), IsDeafened = voiceManager.IsSelfDeaf() });
                            await Send(msg);
                        });
                    });

                }
                else if (lobbyManager.LobbyCount() == 0)
                {
                    var transaction = lobbyManager.GetLobbyCreateTransaction();
                    transaction.SetCapacity((uint)voiceLobbyMessage.VoicePartySize);
                    transaction.SetType(LobbyType.Private);
                    transaction.SetMetadata("FoundryRemoteIp", voiceLobbyMessage.WorldUniqueIdentifier);

                    lobbyManager.CreateLobby(transaction, (Result result, ref Lobby createdLobby) =>
                    {
                        CurrentLobby = createdLobby;
                        Console.WriteLine("Created new Lobby " + CurrentLobby.Value.Id);
                        if (result != Result.Ok)
                        {
                            Console.WriteLine("Couldn't create new Discord lobby");
                            return;
                        }

                        // Connect to voice chat.
                        lobbyManager.ConnectVoice(CurrentLobby.Value.Id, async (_) =>
                        {
                            Console.WriteLine("Connected to voice chat!");
                            var voiceManager = DiscordManager.GetDiscord().GetVoiceManager();
                            var msg = new WebsocketMessage()
                            {
                                Type = RichPresenceMessageTypes.VoiceStatus
                            };
                            msg.From(new VoiceStatusMessage { IsConnected = true, IsMuted = voiceManager.IsSelfMute(), IsDeafened = voiceManager.IsSelfDeaf() });
                            await Send(msg);
                        });
                    });
                }
            });
        }

        private async Task HandleClickedDisconnect()
        {
            if (!CurrentLobby.HasValue)
            {
                Console.WriteLine("No currently connected Lobby, skipping disconnect");
                return;
            }

            var lobbyManager = DiscordManager.GetDiscord().GetLobbyManager();
            lobbyManager.DisconnectLobby(CurrentLobby.Value.Id, (Result result) =>
            {
                Console.WriteLine("Disconnected from Lobby");
                CurrentLobby = null;
            });
        }
    }
}
