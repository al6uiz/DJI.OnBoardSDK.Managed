using System;
using System.Text;

namespace DJI.OnBoardSDK
{
    public static class Utility
    {
        internal static string GetString(byte[] bytes)
        {
            var buffer = new StringBuilder(bytes.Length);

            for (int i = 0; i < bytes.Length; i++)
            {
                var letter = (char)bytes[i];
                if (letter != '\0')
                {
                    buffer.Append(letter);
                }
            }

            return buffer.ToString();
        }

        public static byte[] ParseHex(string hex)
        {
            byte[] bytes = new byte[hex.Length / 2];

            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = ParseByteHex(hex, i * 2);
            }

            return bytes;
        }

        public static byte ParseByteHex(string hex, int index)
        {
            return (byte)((Parse4(hex[index]) << 4) | Parse4(hex[index + 1]));
        }


        private static byte Parse4(char letter)
        {
            if (letter >= '0' && letter <= '9') { return (byte)(letter - '0'); }
            else if (letter >= 'A' && letter <= 'F') { return (byte)(10 + (letter - 'A')); }
            else if (letter >= 'a' && letter <= 'f') { return (byte)(10 + (letter - 'a')); }
            else throw new ArgumentException();
        }
    }


    public abstract class NativeObject
    {
        public Ptr AllocPointer
        {
            get
            {
                byte[] buffer = new byte[TypeSize];
                WriteTo(buffer);
                return buffer;
            }
        }


        public abstract int TypeSize { get; }

        public abstract void ReadFrom(Ptr p, int size = 0);

        public abstract void WriteTo(Ptr p);
    }
}
