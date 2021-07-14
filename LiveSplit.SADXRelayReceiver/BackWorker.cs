using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using LiveSplit.SADXRelayPacketLib;

namespace LiveSplit.SADXRelayReceiver
{
    public static class BackWorker
    {
        private static byte[] _receivedBytes;
        
        public static BackgroundWorker backWorker = new BackgroundWorker();

        public static UdpClient udpClient = new UdpClient("localhost", 3456);
        public static void DoWork()
        {
            int timeout = 2000;

            Packet idPacket = new Packet("R3L4");
            Task<int> sendTask;

            try
            {
                sendTask = udpClient.SendAsync(idPacket);
                sendTask.Wait(timeout);
            }
            catch
            {
                MessageBox.Show("Couldn't connect to the server");
                ReceiverForm.Exit();
            }

            Task<UdpReceiveResult> confirmationTask;
            try
            {
                confirmationTask = udpClient.ReceiveAsync();
                confirmationTask.Wait(timeout);
            }
            catch
            {
                MessageBox.Show("Couldn't get a response from the server");
                ReceiverForm.Exit();
            }

            while (true)
            {
                var receivedResultsTask = udpClient.ReceiveAsync();
                receivedResultsTask.Wait();
                UdpReceiveResult receivedResults = receivedResultsTask.Result;
                
                _receivedBytes = receivedResults.Buffer;
                Packet sent = Packet.FromBytes(ref _receivedBytes);

                Console.WriteLine("received packet");
                backWorker.ReportProgress(0, sent);
            }
        }
    }
}