# KakaoLoco
Loco protocol implementation via c#

# Testing Code
```cs
        public static LocoPacketResponse Checkin()
        {
            LocoPacketResponse getconfRes = Getconf();
            LocoSecureSocket socket = new(Dns.GetHostAddresses((string)getconfRes.body["ticket"]["lsl"][0])[0].ToString(), (int)getconfRes.body["wifi"]["ports"][0]);
            LocoSession session = new(socket);
            LocoPacketResponse response = session.Request(
                "CHECKIN",
                new JObject
                {
                    { "MCCMNC", "999" },
                    { "appVer", "3.3.0" },
                    { "countryISO", "KR" },
                    { "lang", "ko" },
                    { "ntype", 0 },
                    { "os", "win32" },
                    { "useSub", true },
                    { "userId", (long)1 }
                }
            );

            return response;
        }

        public static LocoPacketResponse Getconf()
        {
            LocoNormalSocket socket = new("booking-loco.kakao.com", 443);
            LocoSession session = new(socket);
            LocoPacketResponse response = session.Request(
                "GETCONF",
                new JObject
                {
                    { "MCCMNC", "999" },
                    { "os", "win32" },
                    { "model", "" }
                }
            );

            return response;
        }
```
