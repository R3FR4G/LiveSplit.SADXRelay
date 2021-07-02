using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LiveSplit.SADXRelayServer
{
    class Program
    {
        static void Main(string[] args)
        {
            DoWork();
        }
        
        static async void DoWork()
        {
            using (var udpClient = new UdpClient(3456))
            {
                string response = "";
                while (true)
                {
                    //IPEndPoint object will allow us to read datagrams sent from any source.
                    var receivedResults = await udpClient.ReceiveAsync();
                    response += Encoding.UTF8.GetString(receivedResults.Buffer);
                    if (response.Contains("\n"))
                    {
                        response = response.Split("\n")[0]; break;
                        //check if the connection is allowed (maybe have a connect packet type)
                        //then send back to the receiver if it's allowed
                    }
                }
            }
        }
    }
}