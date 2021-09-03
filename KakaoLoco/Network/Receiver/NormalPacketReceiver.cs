using KakaoLoco.Util;
using System;
using static KakaoLoco.Network.Packet.Packet;

namespace KakaoLoco.Network.Receiver
{
    public class NormalPacketReceiver
    {
        private byte[] currentBytes;
        private int packetLength;

        public NormalPacketReceiver()
        {
            this.currentBytes = Array.Empty<byte>();
            this.packetLength = -1;
        }

        public LocoPacketResponse? Perform(byte[] data)
        {
            this.currentBytes = BytesBuffer.Combine(this.currentBytes, data);

            if (this.currentBytes.Length >= 22 && this.packetLength == -1)
            {
                this.packetLength = (int)BytesBuffer.ReadUInt(this.currentBytes, 18);
            }

            if (this.packetLength != -1)
            {
                if (this.currentBytes.Length >= (this.packetLength + 22))
                {
                    byte[] packetBytes = BytesBuffer.ReadBytes(this.currentBytes, 0, this.packetLength + 22);
                    LocoPacketResponse response = ToLocoPacketResponse(packetBytes);

                    if (this.currentBytes.Length >= packetBytes.Length)
                        this.currentBytes = BytesBuffer.ReadBytes(this.currentBytes, packetBytes.Length, this.currentBytes.Length - packetBytes.Length);
                    else
                        this.currentBytes = Array.Empty<byte>();

                    this.packetLength = -1;

                    return response;
                }
            }

            return null;
        }
    }
}
