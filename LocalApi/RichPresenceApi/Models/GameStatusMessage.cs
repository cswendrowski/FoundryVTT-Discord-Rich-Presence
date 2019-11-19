namespace TestApi.Models
{
    public class GameStatusMessage
    {
        public string Details { get; set; } = "";

        public string State { get; set; } = "";

        public int CurrentPlayerCount { get; set; } = -1;

        public int MaxPlayerCount { get; set; } = -1;

        public string FoundryUrl { get; set; } = "";

        public string WorldUniqueId { get; set; } = "";

        public string SystemName { get; set; } = "";
    }
}
