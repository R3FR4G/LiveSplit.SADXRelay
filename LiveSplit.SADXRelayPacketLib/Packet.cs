using System;
using System.Text;

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
        Response
    }
    public class Packet
    {
        public PacketType Type { get; }
        public TimeSpan Time { get; }
        public string Id { get; }
        public ResponseCode Response { get; }

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

        /// <summary>
        /// Create a Packet of type Disconnect
        /// </summary>
        public Packet()
        {
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
        /// Gets the bytes representation of the Packet 
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            byte[] buffer;
            if (Type == PacketType.NewConnection)
            {
                buffer = new byte[5];
                buffer[0] = (byte)Type;
                Encoding.ASCII.GetBytes(Id).CopyTo(buffer, 1);
                return buffer;
            }

            if (Type == PacketType.CurrentTime)
            {
                buffer = new byte[9];
                buffer[0] = (byte)Type;
                BitConverter.GetBytes(Time.TotalMilliseconds).CopyTo(buffer, 1);
                return buffer;
            }

            if (Type == PacketType.Response)
            {
                buffer = new byte[2];
                buffer[0] = (byte)Type;
                buffer[1] = (byte)Response;
                return buffer;
            }

            return new byte[] { (byte)Type };
        }

        /// <summary>
        /// Returns a new Packet parsed from the bytes
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static Packet FromBytes(ref byte[] bytes)
        {
            PacketType type = (PacketType)bytes[0];
            if (type == PacketType.NewConnection)
            {
                string id = Encoding.ASCII.GetString(bytes, 1, 4);
                return new Packet(id);
            }

            if (type == PacketType.CurrentTime)
            {
                TimeSpan time = TimeSpan.FromMilliseconds(BitConverter.ToDouble(bytes, 1));
                return new Packet(time);
            }

            if (type == PacketType.Response)
            {
                ResponseCode responseCode = (ResponseCode)bytes[1];
                return new Packet(responseCode);
            }

            return new Packet();
        }

        public override string ToString()
        {
            if (Type == PacketType.NewConnection)
                return $"Type: {Type.ToString()}, Id: {Id}";
            if (Type == PacketType.CurrentTime)
                return $"Type: {Type.ToString()}, Time: {Time}";
            if (Type == PacketType.Response)
                return $"Type: {Type.ToString()}, Response: {Response.ToString()}";
            return $"Type: {Type.ToString()}";
        }
    }
}