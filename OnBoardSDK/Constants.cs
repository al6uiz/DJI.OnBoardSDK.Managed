using System;
using System.Runtime.InteropServices;

namespace DJI.OnBoardSDK
{
    public partial class CoreAPI
    {
        public const int HEADER_SIZE = 12;


        // DJI_Config.h
        public const int MEMORY_SIZE = 1024; // unit is byte
        public const int BUFFER_SIZE = 1024;
        public const int ACK_SIZE = 10;





        // DJI_App.h
        public const int MSG_ENABLE_FLAG_LEN = 2;

        public const byte EXC_DATA_SIZE = 16;
        public const byte SET_CMD_SIZE = 2;

        //----------------------------------------------------------------------
        // for cmd agency
        //----------------------------------------------------------------------
        public const ushort REQ_TIME_OUT = 0x0000;
        public const ushort REQ_REFUSE = 0x0001;
        public const ushort CMD_RECIEVE = 0x0002;
        public const ushort STATUS_CMD_EXECUTING = 0x0003;
        public const ushort STATUS_CMD_EXE_FAIL = 0x0004;
        public const ushort STATUS_CMD_EXE_SUCCESS = 0x0005;



        // DJI_Link.h
        public const int ACK_SESSION_IDLE = 0;
        public const int ACK_SESSION_PROCESS = 1;
        public const int ACK_SESSION_USING = 2;
        public const int CMD_SESSION_0 = 0;
        public const int CMD_SESSION_1 = 1;
        public const int CMD_SESSION_AUTO = 32;

        public const int POLL_TICK = 20; // unit is ms
    }
}
