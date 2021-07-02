using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using System.Reflection;
using System.Timers;
using LiveSplit.Model;
using LiveSplit.UI;
using LiveSplit.UI.Components;
using System.Windows.Forms;
using LiveSplit.Options;
using Timer = System.Timers.Timer;

namespace LiveSplit.SADXRelayClient
{
    public class ClientComponent : LogicComponent
    {
        public static Timer updateTimer = new Timer()
        {
            Interval = 1000.0/60.0,
            AutoReset = true,
            Enabled = true
        };
        
        public static ClientSettings settingsControl = new ClientSettings();
        
        public override string ComponentName => ClientFactory.Name;
        public ClientComponent(LiveSplitState state)
        {
            updateTimer.Elapsed += Update;
        }

        private void Update(object sender, ElapsedEventArgs e)
        {
            
        }

        public override Control GetSettingsControl(LayoutMode mode)
        {
            return settingsControl;
        }

        public override XmlNode GetSettings(XmlDocument document)
        {
            return settingsControl.GetSettings(document);
        }

        public override void SetSettings(XmlNode settings)
        {
            settingsControl.SetSettings(settings);
        }
        
        public override void Dispose()
        {
            
        }
        
        public override void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode) { }
    }
}