using KakaoLoco.Network.Receiver;
using KakaoLoco.Network.Secure;
using KakaoLoco.Util;
using System.Net;
using System.Net.Sockets;
using static KakaoLoco.Network.Packet.Packet;

namespace KakaoLoco.Network.Socket
{
    public class LocoSecureSocket : ILocoSocket
    {
        private readonly System.Net.Sockets.Socket client;
        private readonly CryptoManager cryptoManager;
        private readonly SecurePacketReceiver receiver;

        public LocoSecureSocket(string host, int port)
        {
            IPEndPoint remoteEP = new(IPAddress.Parse(host), port);
            this.client = new(IPAddress.Parse(host).AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.client.Connect(remoteEP);

            this.cryptoManager = new();
            this.receiver = new(this.cryptoManager);

            byte[] handshakePacket = this.cryptoManager.ToLocoHandshakePacket();
            this.client.Send(handshakePacket);
        }

        public void Send(byte[] data)
        {
            byte[] encryptedData = this.cryptoManager.ToLocoSecurePacket(data);
            this.client.Send(encryptedData);
        }

        public LocoPacketResponse? Receive()
        {
            byte[] tempData = new byte[256];
            int receivedCount = this.client.Receive(tempData);
            if (0 >= receivedCount) return null;
            byte[] data = BytesBuffer.ReadBytes(tempData, 0, receivedCount);

            return this.receiver.Perform(data);
        }

        public void Close()
        {
            this.client.Close();
        }
    }
}
