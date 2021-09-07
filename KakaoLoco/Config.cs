namespace KakaoLoco
{
    public class Config
    {
        public readonly static Config DefaultConfig = new();

        public readonly string networkOperator;
        public readonly string appVersion;
        public readonly string locoXMLPublicKey;
        public readonly string modelName;
        public readonly string countryISO;
        public readonly string language;
        public readonly string osAgent;
        public readonly string bookingHost;
        public readonly int bookingPort;
        public readonly bool useSubDevice;
        public readonly int networkType;

        public Config(
            string networkOperator = "999",
            string appVersion = "9.3.1",
            string locoXMLPublicKey = "<RSAKeyValue><Modulus>pElgRBx+g7sniYFW7LE8ivrwXShKTRFV8lXNItMXbN5QSC8vJ/cTSOTS619Xv5Zx7xXJIk4EKxtWesEGbgZpEUP2xQ+IeH9oz0JxayEMvvD1nVNAWgpWE4pociEoArsK7qY3YwXb1CiDHo9hojLv7djbo3cwXvlyMh4TUrX2RjCZPlVJxk/LVjzcl9ohJLkl3eoSrf0AE4kQ9mk3+raEhq5Dv+IDxKYX+fIytUWKmrQJusjtre9oVUX5sBOYZ0dzez/XapusEhUWImmB6mciVXfRXQ8IK4IH6vfNyxMSOTfLEhRYN2SMLzplAYFiMV536tLS3VmG5GJRdkpDubqPeQ==</Modulus><Exponent>Aw==</Exponent></RSAKeyValue>",
            string modelName = "",
            string countryISO = "KR",
            string language = "ko",
            string osAgent = "win32",
            string bookingHost = "booking-loco.kakao.com",
            int bookingPort = 443,
            bool useSubDevice = true,
            int networkType = 0
        )
        {
            this.networkOperator = networkOperator;
            this.appVersion = appVersion;
            this.locoXMLPublicKey = locoXMLPublicKey;
            this.modelName = modelName;
            this.countryISO = countryISO;
            this.language = language;
            this.osAgent = osAgent;
            this.bookingHost = bookingHost;
            this.bookingPort = bookingPort;
            this.useSubDevice = useSubDevice;
            this.networkType = networkType;
        }
    }
}
