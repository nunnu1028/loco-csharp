using static KakaoLoco.Network.Packet.Packet;

namespace KakaoLoco.Network.Socket
{
    public interface ILocoSocket
    {
        public void Send(byte[] data);
        public LocoPacketResponse? Receive();
        public void Close();
    }
}
