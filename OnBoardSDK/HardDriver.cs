namespace DJI.OnBoardSDK
{

    public interface IPlatformDriver
    {
        void init();
        long getTimeStamp();
        int send(byte[] buffer, int offset, int len);
        int readall(byte[] buffer, int maxlen);
        bool getDeviceStatus();

        void lockMemory();
        void freeMemory();

        void lockMSG();
        void lockLog();
        void freeMSG();

        void lockACK();
        void freeACK();

        void notify();
        void wait(int timeout);

        void lockProtocolHeader();
        void freeProtocolHeader();

        void lockNonBlockCBAck();
        void freeNonBlockCBAck();

        void notifyNonBlockCBAckRecv();
        void freeLog();
        void nonBlockWait();

#if !NETMF
        void displayLog(string format);
#else
        void displayLog(string format);
        void displayLog(string format, object a0);
        void displayLog(string format, object a0, object a1);
        void displayLog(string format, object a0, object a1, object a2);
        void displayLog(string format, object a0, object a1, object a2, object a3);
        void displayLog(string format, object[] args);
#endif
    }
}
