using System;
using System.Linq;

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

        public void PutUInt(uint value, bool littleEndian = false)
        {
            byte[] uintBytes = BitConverter.GetBytes(value);
            if (littleEndian)
                Array.Reverse(uintBytes);
            this.PutBytes(uintBytes);
        }

        public void PutShort(short value, bool littleEndian = false)
        {
            byte[] shortBytes = BitConverter.GetBytes(value);
            if (littleEndian)
                Array.Reverse(shortBytes);
            this.PutBytes(shortBytes);
        }

        public void PutUShort(ushort value, bool littleEndian = false)
        {
            byte[] ushortBytes = BitConverter.GetBytes(value);
            if (littleEndian)
                Array.Reverse(ushortBytes);
            this.PutBytes(ushortBytes);
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

        public static int ReadInt(byte[] bytes, int index)
        {
            byte[] intBytes = BytesBuffer.ReadBytes(bytes, index, 4);
            return BitConverter.ToInt32(intBytes);
        }

        public static uint ReadUInt(byte[] bytes, int index)
        {
            byte[] uintBytes = BytesBuffer.ReadBytes(bytes, index, 4);
            return BitConverter.ToUInt32(uintBytes);
        }

        public static short ReadShort(byte[] bytes, int index)
        {
            byte[] shortBytes = BytesBuffer.ReadBytes(bytes, index, 4);
            return BitConverter.ToInt16(shortBytes);
        }

        public static ushort ReadUShort(byte[] bytes, int index)
        {
            byte[] ushortBytes = BytesBuffer.ReadBytes(bytes, index, 4);
            return BitConverter.ToUInt16(ushortBytes);
        }

        public static byte ReadByte(byte[] bytes, int offset)
        {
            return bytes[offset];
        }

        public static byte[] ReadBytes(byte[] bytes, int index, int size, bool littleEndian = false)
        {
            byte[] readBytes = new byte[size];
            Array.Copy(bytes, index, readBytes, 0, Math.Min(bytes.Length, size));
            if (littleEndian)
                Array.Reverse(readBytes);
            return readBytes;
        }

        public static byte[] Combine(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }
    }
}
