using System;
using LiveSplit.Model;
using LiveSplit.UI.Components;

namespace LiveSplit.SADXRelayClient
{
    public class ClientFactory : IComponentFactory
    {
        public const string Name = "SADX Relay Race";
        public string ComponentName => Name;
        public string Description => "Component to get your IGT for the relay race CatBoo";

        public ComponentCategory Category => ComponentCategory.Other;

        public IComponent Create(LiveSplitState state) => new ClientComponent(state);

        public string UpdateName { get; }
        public string XMLURL { get; }
        public string UpdateURL { get; }
        public Version Version => new Version(1, 0);

    }
}