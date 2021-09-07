using KakaoLoco.Network.Receiver;
using KakaoLoco.Network.Secure;
using KakaoLoco.Util;
using System.Net.Sockets;
using static KakaoLoco.Network.Packet.Packet;

namespace KakaoLoco.Network.Socket
{
    public class LocoSecureSocket : ILocoSocket
    {
        private readonly TcpClient client;
        private readonly NetworkStream stream;
        private readonly CryptoManager cryptoManager;
        private readonly SecurePacketReceiver receiver;
        private bool handshaked;

        public LocoSecureSocket(string host, int port, Config config)
        {
            this.client = new(host, port);
            this.stream = this.client.GetStream();

            this.cryptoManager = new(config);
            this.receiver = new(this.cryptoManager);
            this.handshaked = false;
        }

        public void Send(byte[] data)
        {
            if (!this.handshaked)
            {
                byte[] handshakePacket = this.cryptoManager.ToLocoHandshakePacket();
                this.stream.Write(handshakePacket);
                this.handshaked = true;
            }
            byte[] encryptedData = this.cryptoManager.ToLocoSecurePacket(data);
            this.stream.Write(encryptedData);
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
            this.client.Close();
        }
    }
}
