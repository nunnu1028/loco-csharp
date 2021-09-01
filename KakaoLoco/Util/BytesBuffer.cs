using System;

namespace KakaoLoco.Util
{
    public class BytesBuffer
    {
        public byte[] bytes { get; }

        private int currentOffset;

        public BytesBuffer(int size)
        {
            this.bytes = new byte[size];
            this.currentOffset = 0;
        }

        public void PutInt(int value, bool littleEndian = false)
        {
            byte[] intBytes = BitConverter.GetBytes(value);
            if (littleEndian)
                Array.Reverse(intBytes);
            this.PutBytes(intBytes);
        }

        public void PutShort(short value)
        {
            byte[] shortBytes = BitConverter.GetBytes(value);
            this.PutBytes(shortBytes);
        }

        public void PutByte(byte value)
        {
            this.bytes[this.currentOffset] = value;
            currentOffset++;
        }

        public void PutBytes(byte[] value)
        {
            Buffer.BlockCopy(value, 0, this.bytes, currentOffset, value.Length);
            currentOffset += value.Length;
        }

        public void AddOffset(int value)
        {
            currentOffset += value;
        }

        public static int ReadInt(byte[] bytes, int index, bool littleEndian = false)
        {
            byte[] intBytes = BytesBuffer.ReadBytes(bytes, index, 4);
            return BitConverter.ToInt32(intBytes);
        }

        public static short ReadShort(byte[] bytes, int index, bool littleEndian = false)
        {
            byte[] shortBytes = BytesBuffer.ReadBytes(bytes, index, 4);
            return BitConverter.ToInt16(shortBytes);
        }

        public static byte ReadByte(byte[] bytes, int offset)
        {
            return bytes[offset];
        }

        public static byte[] ReadBytes(byte[] bytes, int index, int size, bool littleEndian = false)
        {
            byte[] readBytes = new byte[size];
            Array.Copy(bytes, index, readBytes, 0, size);
            if (littleEndian)
                Array.Reverse(readBytes);
            return readBytes;
        }
    }
}
