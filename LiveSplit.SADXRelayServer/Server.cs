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
using System.Threading.Tasks;
using LiveSplit.SADXRelayPacketLib;

namespace LiveSplit.SADXRelayServer
{
    class Program
    {
        public static List<Player> Players = JsonSerializer.Deserialize<List<Player>>(File.ReadAllText("players.json"));
        static void Main(string[] args)
        {
            foreach (Player player in Players)
            {
                Console.WriteLine(player.ToString());
            }
            DoWork().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static async Task DoWork()
        {
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
                        bool found = false;
                        foreach (Player player in Players)
                        {
                            if(player.PlayerConnection == null)
                                continue;

                            if (player.PlayerConnection.Address.ToString() ==
                                receivedResults.RemoteEndPoint.Address.ToString())
                            {
                                found = true;
                                break;
                            }
                                
                        }
                        
                        if(found)
                            Console.WriteLine("allowed connection");
                        else
                            Console.WriteLine("not allowed");
                    }
                }
            }
        }
    }
}