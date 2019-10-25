namespace TestApi.Models
{
    public class PlayerStatus
    {
        public string SceneName { get; set; } = "";

        public string ActorName { get; set; } = "";

        public int CurrentPlayerCount { get; set; } = -1;

        public int MaxPlayerCount { get; set; } = -1;

        public bool IsGm { get; set; } = false;

        public string FoundryUrl { get; set; } = "";

        public string WorldUniqueId { get; set; } = "";

        public string SystemName { get; set; } = "";
    }
}
