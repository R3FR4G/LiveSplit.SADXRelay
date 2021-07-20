using System;
using System.ComponentModel;
using System.Net;
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

        public static TcpClient tcpClient = new TcpClient();
        
        public static void DoWork()
        {
            tcpClient.Connect("localhost", 3456);
            
            if(!tcpClient.Connected)
                MessageBox.Show("Couldn't connect to the server");

            NetworkStream stream = tcpClient.GetStream();

            while (true)
            {
                if (!stream.DataAvailable)
                    continue;

                int typeByte = stream.ReadByte();

                Console.WriteLine("read: " + typeByte);
                
                if (typeByte == -1)
                {
                    Console.WriteLine("failed");
                    continue;
                }

                PacketType type = (PacketType)typeByte;

                byte[] buffer;
                
                switch (type)
                {
                    case PacketType.CurrentTimeToReceiver:
                        buffer = new byte[11];
                        break;
                    case PacketType.RunUpdateToReceiver:
                        buffer = new byte[3];
                        break;
                    default:
                        buffer = new byte[0];
                        break;
                }

                buffer[0] = (byte)type;
                try
                {
                    stream.Read(buffer, 1, buffer.Length - 1);
                    Packet sent = Packet.FromBytes(ref buffer);
                    
                    Console.WriteLine("received packet");
                    backWorker.ReportProgress(0, sent);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
    }
}