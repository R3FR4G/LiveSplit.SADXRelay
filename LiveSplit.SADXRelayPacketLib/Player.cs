using System.Net;

namespace LiveSplit.SADXRelayPacketLib
{
    public enum RelayTeam : byte
    {
        Europe,
        America
    }

    public enum RelayStory : byte
    {
        Sonic,
        Tails,
        Knuckles,
        Amy,
        Big,
        Gamma,
        SuperSonic
    }
    
    public class Player
    {
        public IPEndPoint PlayerConnection { get; set; } = null;
        public bool IsAuthenticated { get; set; } = false;
        public string Id { get; }
        public string Name { get; }
        public RelayTeam Team { get; }
        public RelayStory Story { get; }

        public Player(string id, string name, RelayTeam team, RelayStory story)
        {
            Id = id;
            Name = name;
            Team = team;
            Story = story;
        }

        public override string ToString()
        {
            return $"ID: {Id}, Name: {Name}, Team: {Team.ToString()}, Story: {Story.ToString()}, Authenticated: {IsAuthenticated}";
        }
    }
}