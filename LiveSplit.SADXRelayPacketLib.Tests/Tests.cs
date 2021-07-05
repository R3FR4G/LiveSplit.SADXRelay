using System;
using System.Collections.Generic;

namespace LiveSplit.SADXRelayPacketLib.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            //packet testing stuff
            Packet timepacket = new Packet(new TimeSpan(0, 47, 51));
            Packet connectpacket = new Packet("AYT8");
            Packet disconnectpacket = new Packet();
            Packet responsepacket = new Packet(ResponseCode.BadId);

            byte[] tp = timepacket.ToBytes();
            byte[] cp = connectpacket.ToBytes();
            byte[] dp = disconnectpacket.ToBytes();
            byte[] rp = responsepacket.ToBytes();

            List<byte[]> packets = new List<byte[]>();
            packets.Add(tp);
            packets.Add(cp);
            packets.Add(dp);
            packets.Add(rp);

            List<string> strings = new List<string>();
            
            foreach (var packet in packets)
            {
                string temp = "";
                foreach (var curbyte in packet)
                {
                    temp += curbyte.ToString("X") + " ";
                }
                strings.Add(temp);
            }

            foreach (var VARIABLE in strings)
            {
                Console.WriteLine(VARIABLE);
            }

            Packet tpr = Packet.FromBytes(ref tp);
            Packet cpr = Packet.FromBytes(ref cp);
            Packet dpr = Packet.FromBytes(ref dp);
            Packet rpr = Packet.FromBytes(ref rp);

            Console.WriteLine(tpr.Type + ", " + tpr.Time);
            Console.WriteLine(cpr.Type + ", " + cpr.Id);
            Console.WriteLine(dpr.Type);
            Console.WriteLine(rpr.Type + ", " + rpr.Response);
        }
    }
}