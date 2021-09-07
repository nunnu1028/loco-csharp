using KakaoLoco.Network.Socket;
using Newtonsoft.Json.Linq;
using static KakaoLoco.Network.Packet.Packet;

namespace KakaoLoco.Network
{
    public class LocoEntrance
    {
        public static LocoRequestResponse<LocoSession> CreateNewSession(Config config, long userId)
        {
            LocoRequestResponse<JObject> checkinRes = LocoEntrance.Checkin(config, userId);
            if (!checkinRes.success) return new LocoRequestResponse<LocoSession>(checkinRes.response, null);
            LocoSecureSocket socket = new((string)checkinRes.result["host"], (int)checkinRes.result["port"], config);
            LocoSession session = new(socket);

            return new LocoRequestResponse<LocoSession>(
                checkinRes.response,
                session
            );
        }

        public static LocoRequestResponse<JObject> Booking(Config config)
        {
            LocoNormalSocket socket = new(config.bookingHost, config.bookingPort);
            LocoSession session = new(socket);
            JObject data = new()
            {
                { "MCCMNC", config.networkOperator },
                { "os", config.osAgent },
                { "model", config.modelName }
            };

            LocoPacketResponse response = session.Request("GETCONF", data);
            session.Close();
            if ((int)response.body["status"] != 0) return new LocoRequestResponse<JObject>(response, null);

            return new LocoRequestResponse<JObject>(
                response,
                new JObject
                {
                    { "host", (string)response.body["ticket"]["lsl"][0] },
                    { "port", (int)response.body["wifi"]["ports"][0] }
                }
            );
        }

        public static LocoRequestResponse<JObject> Checkin(Config config, long userId)
        {
            LocoRequestResponse<JObject> bookingRes = LocoEntrance.Booking(config);
            if (!bookingRes.success) return bookingRes;

            LocoSecureSocket socket = new((string)bookingRes.result["host"], (int)bookingRes.result["port"], config);
            LocoSession session = new(socket);
            JObject data = new()
            {
                { "MCCMNC", config.networkOperator },
                { "appVer", config.appVersion },
                { "countryISO", config.countryISO },
                { "lang", config.language },
                { "ntype", config.networkType },
                { "os", config.osAgent },
                { "useSub", config.useSubDevice },
                { "userId", userId }
            };

            LocoPacketResponse response = session.Request("CHECKIN", data);
            session.Close();
            if ((int)response.body["status"] != 0) return new LocoRequestResponse<JObject>(response, null);

            return new LocoRequestResponse<JObject>(
                response,
                new JObject
                {
                    { "host", (string)response.body["host"] },
                    { "port", (int)response.body["port"] }
                }
            );
        }
    }
}
