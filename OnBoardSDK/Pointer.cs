using System;

namespace DJI.OnBoardSDK
{

    public struct Ptr
    {
        public Ptr(byte[] buffer, int offset)
        {
            _buffer = buffer;

            if (offset < 0 || offset > _buffer.Length)
            {
                throw new OutOfMemoryException();
            }
            _offset = offset;
        }


        public Ptr(byte[] buffer)
        {
            _buffer = buffer;
            _offset = 0;
        }

        private int _offset;

        private byte[] _buffer;


        public byte[] Buffer { get { return _buffer; } }

        public int Offset
        {
            get
            {
                return _offset;
            }
            private set
            {
                if (value < 0 || value > _buffer.Length)
                {
                    throw new OutOfMemoryException();
                }

                _offset = value;
            }
        }


        public void SetByte(byte value)
        {
            _buffer[Offset] = value;
        }

        public byte this[int offset]
        {
            get
            {
                return (this + offset).Byte;
            }
            set
            {
                (this + offset).SetByte(value);
            }
        }



        public static int operator -(Ptr x, Ptr y)
        {
            CheckSameTarget(x, y);

            return x.Offset - y.Offset;
        }


        public static Ptr operator ++(Ptr x)
        {
            return (x + 1);
        }

        public static Ptr operator --(Ptr x)
        {
            return (x - 1);
        }

        public static Ptr operator +(Ptr p, int offset)
        {
            return new Ptr(p._buffer, p.Offset + offset);
        }
        public static Ptr operator -(Ptr p, int offset)
        {
            return new Ptr(p._buffer, p.Offset - offset);
        }

        public static implicit operator Ptr(byte[] buffer)
        {
            return new Ptr(buffer);
        }

        public override int GetHashCode()
        {
            return _buffer.GetHashCode() ^ Offset;
        }

        public override bool Equals(object obj)
        {
            if (obj is Ptr)
            {
                var p = (Ptr)obj;

                return this == p;
            }
            return false;
        }

        public static bool operator >(Ptr x, Ptr y)
        {
            CheckSameTarget(x, y);

            return x.Offset > y.Offset;
        }

        private static void CheckSameTarget(Ptr x, Ptr y)
        {
            if (x._buffer != y._buffer) { throw new InvalidOperationException("Source buffer must be same."); };
        }

        public static bool operator <(Ptr x, Ptr y)
        {
            if (x._buffer != y._buffer) { throw new InvalidOperationException("Source buffer must be same."); };

            return x.Offset < y.Offset;
        }

        public static bool operator !=(Ptr x, Ptr y)
        {
            if (x.IsNull && y.IsNull) { return false; }
            if (x.IsNull || y.IsNull) { return true; }


            if (x._buffer != y._buffer) { throw new InvalidOperationException("Source buffer must be same."); };

            return x.Offset != y.Offset;
        }

        public static bool operator ==(Ptr x, Ptr y)
        {
            if (x.IsNull && y.IsNull) { return true; }
            if (x.IsNull || y.IsNull) { return false; }


            if (x._buffer != y._buffer) { throw new InvalidOperationException("Source buffer must be same."); };

            return x.Offset == y.Offset;
        }

        public void Move(Ptr destination, int length)
        {
            Array.Copy(_buffer, Offset, destination._buffer, destination.Offset, length);
        }

        public void Set(byte value, int length)
        {
            for (int i = 0; i < length; i++)
            {
                Buffer[Offset + i] = value;
            }
        }

        public byte Byte
        {
            get { return _buffer[Offset]; }
        }

        public bool IsNull { get { return _buffer == null; } }

        public sbyte SByte
        {
            get
            {
                return (sbyte)_buffer[_offset];
            }
        }

        public short Int16
        {
            get
            {
                return (short)(_buffer[_offset] | (_buffer[_offset + 1] << 8));
            }
        }

        public ushort UInt16
        {
            get
            {
                return (ushort)Int16;

            }
        }

        public int Int32
        {
            get
            {
                return (int)(_buffer[_offset] |
                    (_buffer[_offset + 1] << 8) |
                    (_buffer[_offset + 2] << 16) |
                    (_buffer[_offset + 3] << 24));
            }
        }

        public uint UInt32
        {
            get
            {
                return (uint)Int32;
            }
        }

        public long Int64
        {
            get
            {
                return (long)((long)Int32 | (((long)(this + 4).Int32) << 32));
            }
        }

        public ulong UInt64
        {
            get
            {
                return (ulong)Int64;
            }
        }

        public unsafe float Single
        {
            get
            {
                int temp = Int32;

                return *(float*)&temp;
            }
        }

        public unsafe double Double
        {
            get
            {
                long temp = Int64;

                return *(double*)&temp;
            }
        }

        internal void WriteBytes(byte[] data)
        {
            if (Offset + data.Length > _buffer.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            Array.Copy(data, 0, _buffer, _offset, data.Length);
        }

        public void SetInt16(short value)
        {
            _buffer[_offset] = (byte)(value & 0xFF);
            _buffer[_offset + 1] = (byte)((value >> 8) & 0xFF);
        }

        public void SetInt32(int value)
        {
            _buffer[_offset] = (byte)(value & 0xFF);
            _buffer[_offset + 1] = (byte)((value >> 8) & 0xFF);
            _buffer[_offset + 2] = (byte)((value >> 16) & 0xFF);
            _buffer[_offset + 3] = (byte)((value >> 24) & 0xFF);
        }
        public void SetInt64(long value)
        {
            _buffer[_offset] = (byte)(value & 0xFF);
            _buffer[_offset + 1] = (byte)((value >> 8) & 0xFF);
            _buffer[_offset + 2] = (byte)((value >> 16) & 0xFF);
            _buffer[_offset + 3] = (byte)((value >> 24) & 0xFF);

            _buffer[_offset + 4] = (byte)((value >> 32) & 0xFF);
            _buffer[_offset + 5] = (byte)((value >> 40) & 0xFF);
            _buffer[_offset + 6] = (byte)((value >> 48) & 0xFF);
            _buffer[_offset + 7] = (byte)((value >> 56) & 0xFF);
        }

        public void SetUInt16(ushort value)
        {
            SetInt16((short)value);
        }

        public void SetUInt32(uint value)
        {
            SetInt32((int)value);
        }
        public void SetUInt64(ulong value)
        {
            SetInt64((long)value);
        }
        public unsafe void SetSingle(float x)
        {

#if NETMF
            var raw = BitConverter.GetBytes(x);
            ((Ptr)raw).Move(this, 4);
#else
            int temp = 0;
            *(float*)&temp = x;
            SetInt32(temp);
#endif

        }
        public unsafe void SetDouble(double x)
        {
#if NETMF
            var raw = BitConverter.GetBytes(x);
            ((Ptr)raw).Move(this, 8);
#else
            long temp = 0;
            *(double*)&temp = x;
            SetInt64(temp);
#endif

        }
    }
}