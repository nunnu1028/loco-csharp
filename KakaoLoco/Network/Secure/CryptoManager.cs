using KakaoLoco.Util;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Security.Cryptography;

namespace KakaoLoco.Network.Secure
{
    public class CryptoManager
    {
        private readonly byte[] AESKey;
        private readonly RNGCryptoServiceProvider provider;

        public CryptoManager()
        {
            this.AESKey = new byte[16];
            this.provider = new();
            this.provider.GetBytes(this.AESKey);
        }

        public byte[] AESEncrypt(byte[] bytes, byte[] iv)
        {
            IBufferedCipher cipher = CipherUtilities.GetCipher("AES/CFB/NoPadding");
            cipher.Init(true, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", this.AESKey), iv));

            return cipher.DoFinal(bytes);
        }

        public byte[] ToLocoSecurePacket(byte[] data)
        {
            byte[] iv = this.GetRandomCipherIV();
            byte[] encryptedData = this.AESEncrypt(data, iv);
            BytesBuffer bytesBuffer = new(20 + encryptedData.Length);

            bytesBuffer.PutUInt((uint)(encryptedData.Length + 16));
            bytesBuffer.PutBytes(iv);
            bytesBuffer.PutBytes(encryptedData);

            return bytesBuffer.bytes;
        }

        public byte[] AESDecrypt(byte[] bytes, byte[] iv)
        {
            IBufferedCipher cipher = CipherUtilities.GetCipher("AES/CFB/NoPadding");
            cipher.Init(false, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", this.AESKey), iv));

            return cipher.DoFinal(bytes);
        }

        public byte[] GetRandomCipherIV()
        {
            byte[] iv = new byte[16];
            provider.GetBytes(iv);

            return iv;
        }

        public byte[] GetRSAEncryptedAESKey()
        {
            Asn1Object obj = Asn1Object.FromByteArray(Convert.FromBase64String("MIIBIDANBgkqhkiG9w0BAQEFAAOCAQ0AMIIBCAKCAQEApElgRBx+g7sniYFW7LE8ivrwXShKTRFV8lXNItMXbN5QSC8vJ/cTSOTS619Xv5Zx7xXJIk4EKxtWesEGbgZpEUP2xQ+IeH9oz0JxayEMvvD1nVNAWgpWE4pociEoArsK7qY3YwXb1CiDHo9hojLv7djbo3cwXvlyMh4TUrX2RjCZPlVJxk/LVjzcl9ohJLkl3eoSrf0AE4kQ9mk3+raEhq5Dv+IDxKYX+fIytUWKmrQJusjtre9oVUX5sBOYZ0dzez/XapusEhUWImmB6mciVXfRXQ8IK4IH6vfNyxMSOTfLEhRYN2SMLzplAYFiMV536tLS3VmG5GJRdkpDubqPeQIBAw=="));

            DerSequence publicKeySequence = (DerSequence)obj;

            DerBitString encodedPublicKey = (DerBitString)publicKeySequence[1];
            DerSequence publicKey = (DerSequence)Asn1Object.FromByteArray(encodedPublicKey.GetBytes());

            DerInteger modulus = (DerInteger)publicKey[0];
            DerInteger exponent = (DerInteger)publicKey[1];

            RsaKeyParameters keyParameters = new(false, modulus.PositiveValue, exponent.PositiveValue);
            IBufferedCipher cipher = CipherUtilities.GetCipher("RSA/ECB/OAEPWithSHA1AndMGF1Padding");
            cipher.Init(true, keyParameters);
            
            return cipher.DoFinal(this.AESKey);
        }

        public byte[] ToLocoHandshakePacket()
        {
            byte[] encryptedData = this.GetRSAEncryptedAESKey();
            BytesBuffer bytesBuffer = new(12 + encryptedData.Length);

            bytesBuffer.PutUInt((uint)encryptedData.Length);
            bytesBuffer.PutUInt(12);
            bytesBuffer.PutUInt(2);
            bytesBuffer.PutBytes(encryptedData);

            return bytesBuffer.bytes;
        }
    }
}