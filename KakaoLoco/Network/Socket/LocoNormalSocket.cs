using KakaoLoco.Network.Receiver;
using KakaoLoco.Util;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using static KakaoLoco.Network.Packet.Packet;

namespace KakaoLoco.Network.Socket
{
    public class LocoNormalSocket : ILocoSocket
    {
        private readonly TcpClient client;
        private readonly SslStream stream;
        private readonly NormalPacketReceiver receiver;

        public LocoNormalSocket(string host, int port)
        {
            this.client = new(host, port);
            this.stream = new(client.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
            this.stream.AuthenticateAsClient(host);
            this.receiver = new();
        }

        public void Send(byte[] data)
        {
            this.stream.Write(data, 0, data.Length);
        }

        public LocoPacketResponse? Receive()
        {
            byte[] tempData = new byte[256];
            int receivedCount = this.stream.Read(tempData);
            if (0 >= receivedCount) return null;
            byte[] data = BytesBuffer.ReadBytes(tempData, 0, receivedCount);

            return this.receiver.Perform(data);
        }

        public void Close()
        {
            this.stream.Close();
            this.client.Close();
        }

        private static bool ValidateServerCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors
        )
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            return false;
        }
    }
}
