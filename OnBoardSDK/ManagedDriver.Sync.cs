using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace DJI.OnBoardSDK
{
    partial class ManagedDriver
    {
        private object _lockAck = new object();
        private object _lockMessage = new object();
        private object _lockMemory = new object();
        private object _lockCallback = new object();
        private object _lockHeader = new object();
        private object _lockLog = new object();

        private AutoResetEvent _signalNotify = new AutoResetEvent(false);

        private void WriteSyncLog(string message)
        {
            //displayLog(Thread.CurrentThread.ManagedThreadId + " : " + message);
        }

        public void lockACK()
        {
            WriteSyncLog("lockAck");
            Monitor.Enter(_lockAck);
        }

        public void freeACK()
        {
            WriteSyncLog("freeAck");
            Monitor.Exit(_lockAck);
        }


        public void lockLog()
        {
            WriteSyncLog("lockLog");
            Monitor.Enter(_lockLog);
        }

        public void freeLog()
        {
            WriteSyncLog("freeLog");
            Monitor.Exit(_lockLog);
        }


        public void lockMemory()
        {
            WriteSyncLog("lockMemory");
            Monitor.Enter(_lockMemory);

        }

        public void freeMemory()
        {
            WriteSyncLog("freeMemory");
            Monitor.Exit(_lockMemory);

        }


        public void lockMSG()
        {

            //WriteSyncLog("lockMsg");
            Monitor.Enter(_lockMessage);
        }

        public void freeMSG()
        {
            //WriteSyncLog("freeMsg");
            Monitor.Exit(_lockMessage);
        }


        public void lockNonBlockCBAck()
        {
            WriteSyncLog("lockNB");
            Monitor.Enter(_lockCallback);
        }

        public void freeNonBlockCBAck()
        {
            WriteSyncLog("freeNB");
            Monitor.Exit(_lockCallback);
        }


        public void lockProtocolHeader()
        {
            WriteSyncLog("lockheader");
            Monitor.Enter(_lockHeader);
        }

        public void freeProtocolHeader()
        {
            WriteSyncLog("freeHeader");
            Monitor.Exit(_lockHeader);
        }


        public void nonBlockWait()
        {
            //throw new NotImplementedException();
        }

        public void notifyNonBlockCBAckRecv()
        {
            //throw new NotImplementedException();
        }

        public void wait(int timeout)
        {
            WriteSyncLog("wait");
            var result = _signalNotify.WaitOne(timeout, false);

            if (!result)
            {
                WriteSyncLog("Wait timeout");
            }
        }

        public void notify()
        {
            WriteSyncLog("notify");
            _signalNotify.Set();
        }
    }
}
