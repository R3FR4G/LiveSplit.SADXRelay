using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SADXRelayPacketLib
{
    public enum ResponseCode : byte
    {
        Ok,
        Error,
        BadId
    }
    public enum PacketType : byte
    {
        NewConnection,
        CurrentTime,
        Disconnect,
        Response,
        RunUpdate,
        
        CurrentTimeToReceiver,
        RunUpdateToReceiver
    }
    public class Packet
    {
        public PacketType Type { get; }
        public TimeSpan Time { get; }
        public string Id { get; }
        public ResponseCode Response { get; }
        
        public RelayStory PlayerIndex { get; }
        public RelayTeam PlayerTeamIndex { get; }

        /// <summary>
        /// Create a Packet of type NewConnection
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="ArgumentException"></exception>
        public Packet(string id)
        {
            if (id.Length != 4)
                throw new ArgumentException("The ID must be a 4 characters string", nameof(id));
            Type = PacketType.NewConnection;
            Id = id;
        }

        /// <summary>
        /// Create a Packet of type CurrentTime
        /// </summary>
        /// <param name="time"></param>
        public Packet(TimeSpan time)
        {
            Type = PacketType.CurrentTime;
            Time = time;
        }

        
        public Packet(bool isRunUpdate = false)
        {
            if (isRunUpdate)
                Type = PacketType.RunUpdate;
            else
                Type = PacketType.Disconnect;
        }

        /// <summary>
        /// Create a Packet of type Response
        /// </summary>
        /// <param name="responseCode"></param>
        public Packet(ResponseCode responseCode)
        {
            Type = PacketType.Response;
            Response = responseCode;
        }

        /// <summary>
        /// Create a Packet of type CurrentTimeToReceiver
        /// </summary>
        /// <param name="responseCode"></param>
        public Packet(RelayStory playerIndex, RelayTeam playerTeamIndex, TimeSpan time)
        {
            Type = PacketType.CurrentTimeToReceiver;
            PlayerIndex = playerIndex;
            PlayerTeamIndex = playerTeamIndex;
            Time = time;
        }

        /// <summary>
        /// Create a Packet of type RunUpdateToReceiver
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <param name="playerTeamIndex"></param>
        public Packet(RelayStory playerIndex, RelayTeam playerTeamIndex)
        {
            Type = PacketType.RunUpdateToReceiver;
            PlayerIndex = playerIndex;
            PlayerTeamIndex = playerTeamIndex;
        }
        
        /// <summary>
        /// Gets the bytes representation of the Packet 
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            byte[] buffer;
            switch (Type)
            {
                case PacketType.NewConnection:
                {
                    buffer = new byte[5];
                    buffer[0] = (byte) Type;
                    Encoding.ASCII.GetBytes(Id).CopyTo(buffer, 1);
                    return buffer;
                }
                case PacketType.CurrentTime:
                {
                    buffer = new byte[9];
                    buffer[0] = (byte)Type;
                    BitConverter.GetBytes(Time.TotalMilliseconds).CopyTo(buffer, 1);
                    return buffer;
                }
                case PacketType.Response:
                {
                    buffer = new byte[2];
                    buffer[0] = (byte)Type;
                    buffer[1] = (byte)Response;
                    return buffer;
                }
                case PacketType.RunUpdate:
                {
                    return new byte[] { (byte)Type };
                }
                case PacketType.CurrentTimeToReceiver:
                {
                    buffer = new byte[11];
                    buffer[0] = (byte)Type;
                    buffer[1] = (byte)PlayerIndex;
                    buffer[2] = (byte)PlayerTeamIndex;
                    BitConverter.GetBytes(Time.TotalMilliseconds).CopyTo(buffer, 3);
                    return buffer;
                }
                case PacketType.RunUpdateToReceiver:
                {
                    buffer = new byte[3];
                    buffer[0] = (byte)Type;
                    buffer[1] = (byte)PlayerIndex;
                    buffer[2] = (byte)PlayerTeamIndex;
                    return buffer;
                }
                default:
                    return new byte[] { (byte)Type };
            }
        }

        /// <summary>
        /// Returns a new Packet parsed from the bytes
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static Packet FromBytes(ref byte[] bytes)
        {
            PacketType type = (PacketType)bytes[0];
            
            switch (type)
            {
                case PacketType.NewConnection:
                {
                    string id = Encoding.ASCII.GetString(bytes, 1, 4);
                    return new Packet(id);
                }
                case PacketType.CurrentTime:
                {
                    TimeSpan time = TimeSpan.FromMilliseconds(BitConverter.ToDouble(bytes, 1));
                    return new Packet(time);
                }
                case PacketType.Response:
                {
                    ResponseCode responseCode = (ResponseCode)bytes[1];
                    return new Packet(responseCode);
                }
                case PacketType.RunUpdate:
                {
                    return new Packet(true);
                }
                case PacketType.CurrentTimeToReceiver:
                {
                    RelayStory playerIndex = (RelayStory)bytes[1];
                    RelayTeam playerTeamIndex = (RelayTeam)bytes[2];
                    TimeSpan time = TimeSpan.FromMilliseconds(BitConverter.ToDouble(bytes, 3));
                    return new Packet(playerIndex, playerTeamIndex, time);
                }
                case PacketType.RunUpdateToReceiver:
                {
                    RelayStory playerIndex = (RelayStory)bytes[1];
                    RelayTeam playerTeamIndex = (RelayTeam)bytes[2];
                    return new Packet(playerIndex, playerTeamIndex);
                }
                default:
                    return new Packet();
            }
        }

        public override string ToString()
        {
            switch (Type)
            {
                case PacketType.NewConnection:
                    return $"Type: {Type.ToString()}, Id: {Id}";
                case PacketType.CurrentTime:
                    return $"Type: {Type.ToString()}, Time: {Time}";
                case PacketType.Response:
                    return $"Type: {Type.ToString()}, Response: {Response.ToString()}";
                case PacketType.RunUpdate:
                    return $"Type: {Type.ToString()}";
                case PacketType.CurrentTimeToReceiver:
                    return $"Type: {Type.ToString()}, PIndex: {PlayerIndex}, PTeamIndex: {PlayerTeamIndex}, Time: {Time}";
                case PacketType.RunUpdateToReceiver:
                    return $"Type: {Type.ToString()}, PIndex: {PlayerIndex}, PTeamIndex: {PlayerTeamIndex}";
                default:
                    return $"Type: {Type.ToString()}";
            }
        }
    }

    public static class UdpClientExtensions
    {
        public static async Task<int> SendAsync(this UdpClient client, Packet packet)
        {
            byte[] bytesToSend = packet.ToBytes();
            return await client.SendAsync(bytesToSend, bytesToSend.Length);
        }

        public static async Task<int> SendAsync(this UdpClient client, Packet packet, IPEndPoint endPoint)
        {
            byte[] bytesToSend = packet.ToBytes();
            return await client.SendAsync(bytesToSend, bytesToSend.Length, endPoint);
        }
    }
}