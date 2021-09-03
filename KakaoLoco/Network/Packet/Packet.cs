using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Bson;
using System.IO;
using KakaoLoco.Util;
using System.Text;
using Newtonsoft.Json;

namespace KakaoLoco.Network.Packet
{
    public class Packet
    {
        private readonly static short statusCode = 0;
        private readonly static byte bodyType = 0;

        private static byte[] ToBSON(JObject value)
        {
            using MemoryStream ms = new();
            using BsonDataWriter dataWriter = new(ms);
            JsonSerializer serializer = new();
            serializer.Serialize(dataWriter, value);
            return ms.ToArray();
        }

        private static JObject ToJSON(byte[] value)
        {
            using MemoryStream ms = new(value);
            using BsonDataReader reader = new(ms);
            JsonSerializer serializer = new();
            return serializer.Deserialize<JObject>(reader);
        }

        public static byte[] ToLocoPacketRequest(int packetID, string method, JObject body)
        {
            byte[] bsonData = ToBSON(body);
            BytesBuffer bytesBuffer = new(22 + bsonData.Length);

            bytesBuffer.PutUInt((uint)packetID);
            bytesBuffer.PutUShort((ushort)Packet.statusCode);

            byte[] methodBytes = Encoding.ASCII.GetBytes(method);
            bytesBuffer.PutBytes(methodBytes);
            bytesBuffer.AddOffset(11 - methodBytes.Length);

            bytesBuffer.PutByte(Packet.bodyType);
            bytesBuffer.PutUInt((uint)bsonData.Length);
            bytesBuffer.PutBytes(bsonData);

            return bytesBuffer.bytes;
        }

        public static LocoPacketResponse ToLocoPacketResponse(byte[] packetBytes)
        {
            return new LocoPacketResponse(packetBytes);
        }

        public struct LocoPacketResponse
        {
            public readonly int packetID;
            public readonly short statusCode;
            public readonly string method;
            public readonly byte bodyType;
            public readonly int bodyLength;
            public readonly JObject body;

            public LocoPacketResponse(byte[] packetBytes)
            {
                this.packetID = BytesBuffer.ReadInt(packetBytes, 0);
                this.statusCode = BytesBuffer.ReadShort(packetBytes, 4);
                this.method = Encoding.UTF8.GetString(BytesBuffer.ReadBytes(packetBytes, 6, 11)).Replace("\0", "");
                this.bodyType = BytesBuffer.ReadByte(packetBytes, 17);
                this.bodyLength = BytesBuffer.ReadInt(packetBytes, 18);
                this.body = Packet.ToJSON(BytesBuffer.ReadBytes(packetBytes, 22, bodyLength));
            }
        }
    }
}
