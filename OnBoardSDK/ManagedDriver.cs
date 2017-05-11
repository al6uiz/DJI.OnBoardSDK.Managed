using System;
#if !NETMF
using System.Diagnostics;
#endif

namespace DJI.OnBoardSDK
{
    public partial class ManagedDriver : IPlatformDriver
    {
#if !NETMF
        public void displayLog(string buf)
        {
            Debug.WriteLine(buf);
        }
#endif
        public bool getDeviceStatus()
        {
            if (SerialDevice == null) return false;
            return SerialDevice.IsOpen == true;
        }

#if !NETMF
        public long getTimeStamp()
        {
            return Environment.TickCount;
        }
#endif

        public void Initialize()
        {
            init();
        }



        public void Release()
        {
            if (SerialDevice != null && SerialDevice.IsOpen == true)
            {
                SerialDevice.Close();
            }
        }


        public ISerialDriver SerialDevice { get; set; }

        public void init()
        {
            SerialDevice.Open();
        }

        public int readall(byte[] buf, int maxlen)
        {
            return SerialDevice.Read(buf);
        }


        public int send(byte[] buf, int offset, int len)
        {
            if (!SerialDevice.IsOpen)
            {
                return -1;
            }
            try
            {
                return SerialDevice.Write(buf, offset, len);
            }
            catch
            {
                return 0;
            }
        }



    }


    public interface ISerialDriver
    {
        bool IsOpen { get; }
        bool Open();

        void Close();

        int Read(byte[] buffer);

        int Write(byte[] buffer, int offset, int length);

    }
}
