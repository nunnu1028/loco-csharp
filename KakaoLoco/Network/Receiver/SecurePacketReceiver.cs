using KakaoLoco.Network.Secure;
using KakaoLoco.Util;
using System;
using static KakaoLoco.Network.Packet.Packet;

namespace KakaoLoco.Network.Receiver
{
    public class SecurePacketReceiver
    {
        private readonly CryptoManager cryptoManager;
        private readonly NormalPacketReceiver receiver;
        private byte[] currentBytes;
        private int packetLength;

        public SecurePacketReceiver(CryptoManager manager)
        {
            this.cryptoManager = manager;
            this.receiver = new();
            this.currentBytes = Array.Empty<byte>();
            this.packetLength = -1;
        }

        public LocoPacketResponse? Perform(byte[] data)
        {
            this.currentBytes = BytesBuffer.Combine(this.currentBytes, data);

            if (this.currentBytes.Length >= 4 && this.packetLength == -1)
            {
                this.packetLength = (int)BytesBuffer.ReadUInt(this.currentBytes, 0);
            }

            if (this.packetLength != -1)
            {
                if (this.currentBytes.Length >= (this.packetLength + 4))
                {
                    byte[] iv = BytesBuffer.ReadBytes(this.currentBytes, 4, 16);
                    byte[] encryptedBytes = BytesBuffer.ReadBytes(this.currentBytes, 20, this.packetLength - 16);
                    byte[] packetBytes = this.cryptoManager.AESDecrypt(encryptedBytes, iv);

                    LocoPacketResponse? response = this.receiver.Perform(packetBytes);

                    if (this.currentBytes.Length > this.packetLength + 4)
                        this.currentBytes = BytesBuffer.ReadBytes(this.currentBytes, this.packetLength + 4, this.currentBytes.Length - this.packetLength + 4);
                    else
                        this.currentBytes = Array.Empty<byte>();

                    this.packetLength = -1;

                    if (response != null) return response;
                }
            }

            return null;
        }
    }
}
