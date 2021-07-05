using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using LiveSplit.Model;
using LiveSplit.UI;
using LiveSplit.UI.Components;
using System.Windows.Forms;
using LiveSplit.Options;
using LiveSplit.SADXRelayPacketLib;
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
        
        public ClientSettings settingsControl = new ClientSettings();

        public static UdpClient ServerClient = new UdpClient("localhost", 3456);

        public static bool IsAuthenticated = false;
        
        public override string ComponentName => ClientFactory.Name;
        public ClientComponent(LiveSplitState state)
        {
            Task.Factory.StartNew(() => MessageBox.Show("SADX Relay Race component running,\nremember to connect to the server in the component settings"));
            updateTimer.Elapsed += Update;
        }

        public static ResponseCode VerifyId(string id)
        {
            int timeout = 2000;
            
            byte[] idPacketBytes = new Packet(id).ToBytes();
            Task<int> sendTask;

            try
            {
                sendTask = ServerClient.SendAsync(idPacketBytes, idPacketBytes.Length);
                sendTask.Wait(timeout);
            }
            catch
            {
                return ResponseCode.Error;
            }

            Task<UdpReceiveResult> confirmationTask;
            try
            {
                confirmationTask = ServerClient.ReceiveAsync();
                confirmationTask.Wait(timeout);
            }
            catch (Exception e)
            {
                return ResponseCode.Error;
            }

            if (!confirmationTask.IsCompleted)
                return ResponseCode.Error;
                
            byte[] confirmationBytes = confirmationTask.Result.Buffer;
            Packet confirmation = Packet.FromBytes(ref confirmationBytes);
            Debug.Assert(confirmation.Type == PacketType.Response);

            return confirmation.Response;
            
        }
        
        private void Update(object sender, ElapsedEventArgs e)
        {
            if (IsAuthenticated)
            {
                
            }
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