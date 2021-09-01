using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Bson;
using System.IO;
using KakaoLoco.Util;
using System.Text;

namespace KakaoLoco.Network.Packet
{
    public class Packet
    {
        private static readonly short default_statusCode = 0;
        private static readonly byte default_BodyType = 0;
        public static byte[] ToBSON(JObject value)
        {
            using MemoryStream ms = new();
            using BsonWriter writer = new(ms);
            value.WriteTo(writer);
            return ms.ToArray();
        }

        public static JObject ToJSON(byte[] value)
        {
            using MemoryStream ms = new(value);
            using BsonReader reader = new(ms);
            JToken token = JToken.ReadFrom(reader);
            return (JObject)token;
        }

        public static byte[] ToLocoPacketRequest(int packetID, string method, JObject body)
        {
            byte[] bsonData = ToBSON(body);
            BytesBuffer bytesBuffer = new(22 + bsonData.Length);

            bytesBuffer.PutInt(packetID, false);
            bytesBuffer.PutShort(default_statusCode);

            byte[] methodBytes = Encoding.ASCII.GetBytes(method);
            bytesBuffer.PutBytes(methodBytes);
            bytesBuffer.AddOffset(11 - methodBytes.Length);

            bytesBuffer.PutByte(default_BodyType);
            bytesBuffer.PutInt(bsonData.Length, false);
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
