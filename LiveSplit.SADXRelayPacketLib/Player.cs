namespace LiveSplit.SADXRelayPacketLib
{
    public enum RelayTeam : byte
    {
        Europe,
        America
    }
    
    public class Player
    {
        public bool IsAuthenticated { get; set; } = false;
        public string Id { get; }
        public string Name { get; }
        public RelayTeam Team { get; }

        public Player(string id, string name, RelayTeam team)
        {
            Id = id;
            Name = name;
            Team = team;
        }

        public override string ToString()
        {
            return $"ID: {Id}, Name: {Name}, Team: {Team.ToString()}, Authenticated: {IsAuthenticated}";
        }
    }
}