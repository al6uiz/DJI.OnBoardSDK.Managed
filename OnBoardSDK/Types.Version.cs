using System;

namespace DJI.OnBoardSDK
{

    public struct Version
#if !NETMF
        : IEquatable<Version>,IComparable
#endif
    {
        public static readonly Version M100_23 = new Version(2, 3, 10, 0);
        public static readonly Version M100_31 = new Version(3, 1, 10, 0);
        public static readonly Version A3_31 = new Version(3, 1, 100, 0);
        public static readonly Version A3_32 = new Version(3, 2, 0, 0);


        public static Version Zero { get { return new Version(0, 0, 0, 0); } }

        public Version(int a, int b, int c, int d) : this((byte)a, (byte)b, (byte)c, (byte)d) { }
        public Version(byte a, byte b, byte c, byte d)
        {
            _data = (int)(a << 24) | ((b << 16) & 0x00ff0000) | ((c << 8) & 0x0000ff00) | (d & 0x000000ff);
        }
        private int _data;

        public int RawVersion { get { return _data; } }

        public override int GetHashCode()
        {
            return _data.GetHashCode();
        }


#if !NETMF
        public override bool Equals(object obj)
        {
            if (obj is Version)
            {
                return Equals((Version)obj);
            }
            return false;
        }
        
        public int CompareTo(object obj)
        {
            if (obj is Version)
            {
                return _data.CompareTo(((Version)obj)._data);
            }
            else return -1;
        }
        public bool Equals(Version other)
        {
            return _data == other._data;
}
#else
        public override bool Equals(object obj)
        {
            if (obj is Version)
            {
                return _data.Equals(((Version)obj)._data);
            }
            return false;
        }
#endif

        public static bool operator ==(Version x, Version y)
        {
            return x._data == y._data;
        }


        public static bool operator !=(Version x, Version y)
        {
            return x._data != y._data;
        }
        public static bool operator >(Version x, Version y)
        {
            return x._data > y._data;
        }

        public static bool operator <(Version x, Version y)
        {
            return x._data < y._data;
        }
    }

}
