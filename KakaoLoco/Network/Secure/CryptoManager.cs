using KakaoLoco.Util;
using System;
using System.IO;
using System.Security.Cryptography;

namespace KakaoLoco.Network.Secure
{
    public class CryptoManager
    {
        private readonly byte[] AESKey;
        private readonly RNGCryptoServiceProvider provider;
        private readonly RijndaelManaged cryptor;

        public CryptoManager()
        {
            this.AESKey = new byte[16];
            this.provider = new();
            this.provider.GetBytes(this.AESKey);

            cryptor = new RijndaelManaged()
            {
                BlockSize = 128,
                KeySize = 128,
                Mode = CipherMode.CFB,
                Padding = PaddingMode.None
            };
        }

        public byte[] AESEncrypt(byte[] bytes, byte[] iv)
        {
            using ICryptoTransform transform = this.cryptor.CreateEncryptor(this.AESKey, iv);
            using NoPaddingTransformWrapper cryptoTransform = new(transform);
            using MemoryStream memoryStream = new();
            using CryptoStream cryptoStream = new(memoryStream, cryptoTransform, CryptoStreamMode.Write);

            cryptoStream.Write(bytes, 0, bytes.Length);
            cryptoStream.FlushFinalBlock();

            return memoryStream.ToArray();
        }

        public byte[] ToLocoSecurePacket(byte[] data)
        {
            byte[] iv = this.GetRandomCipherIV();
            byte[] encryptedData = this.AESEncrypt(data, iv);
            BytesBuffer bytesBuffer = new(20 + encryptedData.Length);

            bytesBuffer.PutInt(encryptedData.Length + 16);
            bytesBuffer.PutBytes(iv);
            bytesBuffer.PutBytes(encryptedData);

            return bytesBuffer.bytes;
        }

        public byte[] AESDecrypt(byte[] bytes, byte[] iv)
        {
            using ICryptoTransform transform = this.cryptor.CreateDecryptor(this.AESKey, iv);
            using NoPaddingTransformWrapper cryptoTransform = new(transform);
            using MemoryStream memoryStream = new();
            using CryptoStream cryptoStream = new(memoryStream, cryptoTransform, CryptoStreamMode.Write);

            cryptoStream.Write(bytes, 0, bytes.Length);
            cryptoStream.FlushFinalBlock();

            return memoryStream.ToArray();
        }

        public byte[] GetRandomCipherIV()
        {
            byte[] iv = new byte[16];
            provider.GetBytes(iv);

            return iv;
        }

        public byte[] GetRSAEncryptedAESKey()
        {
            RSACryptoServiceProvider RSAProvider = new();

            RSAProvider.FromXmlString(Config.locoXMLPublicKey);
            return RSAProvider.Encrypt(this.AESKey, RSAEncryptionPadding.OaepSHA1);
        }

        public byte[] ToLocoHandshakePacket()
        {
            byte[] encryptedData = this.GetRSAEncryptedAESKey();
            BytesBuffer bytesBuffer = new(12 + encryptedData.Length);

            bytesBuffer.PutInt(encryptedData.Length);
            bytesBuffer.PutInt(12);
            bytesBuffer.PutInt(2);
            bytesBuffer.PutBytes(encryptedData);

            return bytesBuffer.bytes;
        }
        public class NoPaddingTransformWrapper : ICryptoTransform
        {

            private ICryptoTransform m_Transform;

            public NoPaddingTransformWrapper(ICryptoTransform symmetricAlgoTransform)
            {
                if (symmetricAlgoTransform == null)
                    throw new ArgumentNullException(nameof(symmetricAlgoTransform));

                this.m_Transform = symmetricAlgoTransform;
            }

            #region simple wrap

            public bool CanReuseTransform
            {
                get { return this.m_Transform.CanReuseTransform; }
            }

            public bool CanTransformMultipleBlocks
            {
                get { return this.m_Transform.CanTransformMultipleBlocks; }
            }

            public int InputBlockSize
            {
                get { return this.m_Transform.InputBlockSize; }
            }

            public int OutputBlockSize
            {
                get { return this.m_Transform.OutputBlockSize; }
            }

            public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
            {
                return this.m_Transform.TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
            }

            #endregion

            public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
            {
                if (inputCount % this.m_Transform.InputBlockSize == 0)
                    return this.m_Transform.TransformFinalBlock(inputBuffer, inputOffset, inputCount);
                else
                {
                    byte[] lastBlocks = new byte[inputCount / this.m_Transform.InputBlockSize + this.m_Transform.InputBlockSize];
                    Buffer.BlockCopy(inputBuffer, inputOffset, lastBlocks, 0, inputCount);
                    byte[] result = this.m_Transform.TransformFinalBlock(lastBlocks, 0, lastBlocks.Length);
                    Array.Resize(ref result, inputCount);
                    return result;
                }
            }

            public void Dispose()
            {
                this.m_Transform.Dispose();
            }
        }
    }
}