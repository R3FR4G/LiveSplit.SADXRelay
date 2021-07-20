using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using LiveSplit.SADXRelayPacketLib;

namespace LiveSplit.SADXRelayServer
{
    class Server
    {
        public static List<Player> Players = JsonSerializer.Deserialize<List<Player>>(File.ReadAllText("players.json"));

        public static NetworkStream receiver;

        public static TcpListener listener = new TcpListener(new IPEndPoint(IPAddress.Any, 3456));

        static void Main(string[] args)
        {
            foreach (Player player in Players)
            {
                Console.WriteLine(player.ToString());
            }
            DoWork().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static async Task InitiateReceiverConnection()
        {
            listener.Start();

            while (true)
            {
                if (!listener.Pending())
                {
                    await Task.Delay(500);
                    continue;
                }

                TcpClient receiverClient = await listener.AcceptTcpClientAsync();
                receiver = receiverClient.GetStream();
                Console.WriteLine("Host connected");
                break;
            }
        }

        static async Task DoWork()
        {
            Task.Factory.StartNew(InitiateReceiverConnection);
            
            using (var udpClient = new UdpClient(3456))
            {
                byte[] receivedBytes;
                while (true)
                {
                    //IPEndPoint object will allow us to read datagrams sent from any source.
                    var receivedResults = await udpClient.ReceiveAsync();
                    receivedBytes = receivedResults.Buffer;
                    Packet sent = Packet.FromBytes(ref receivedBytes);
                    Console.WriteLine(sent.ToString());

                    if (sent.Type == PacketType.NewConnection)
                    {
                        Player player = Players.FirstOrDefault(p => p.Id == sent.Id);
                        if (player != null)
                        {
                            Player oldPlayer = CheckConnection(ref receivedResults);
                            if (oldPlayer != null)
                            {
                                oldPlayer.IsAuthenticated = false;
                                oldPlayer.PlayerConnection = null;
                            }
                            
                            player.IsAuthenticated = true;
                            player.PlayerConnection = receivedResults.RemoteEndPoint;


                            await udpClient.SendAsync(new Packet(ResponseCode.Ok), receivedResults.RemoteEndPoint);
                        }
                        else
                        {
                            await udpClient.SendAsync(new Packet(ResponseCode.BadId), receivedResults.RemoteEndPoint);
                        }
                    }
                    if (sent.Type == PacketType.CurrentTime)
                    {
                        Player sender = CheckConnection(ref receivedResults);

                        if (sender == null || receiver == null)
                            continue;

                        //int toReceiverTaskResult = await udpClient.SendAsync(new Packet(sender.Story, sender.Team, sent.Time), receiver);
                        await receiver.WriteAsync(new Packet(sender.Story, sender.Team, sent.Time).ToBytes(), CancellationToken.None);
                        //Console.WriteLine($"To Receiver Result: {toReceiverTaskResult}");
                    }

                    if (sent.Type == PacketType.RunUpdate)
                    {
                        Player sender = CheckConnection(ref receivedResults);

                        if (sender == null || receiver == null)
                            continue;

                        //int toReceiverTaskResult = await udpClient.SendAsync(new Packet(sender.Story, sender.Team), receiver);
                        await receiver.WriteAsync(new Packet(sender.Story, sender.Team).ToBytes(), CancellationToken.None);
                        //Console.WriteLine($"Sent To Receiver Result: {toReceiverTaskResult}");
                    }
                }
            }
        }

        static Player CheckConnection(ref UdpReceiveResult receivedResults)
        {
            Player sender = null;
            foreach (Player player in Players)
            {
                if(player.PlayerConnection == null)
                    continue;

                if (player.PlayerConnection.Address.ToString() ==
                    receivedResults.RemoteEndPoint.Address.ToString())
                {
                    sender = player;
                    break;
                }
            }

            return sender;
        }
    }
}