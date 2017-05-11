using System;
using System.Collections;
using System.Text;

namespace DJI.OnBoardSDK
{
    partial class ManagedDriver
    {
        public long getTimeStamp()
        {
            return Microsoft.SPOT.Hardware.Utility.GetMachineTime().Ticks / TimeSpan.TicksPerMillisecond;
        }

        private ArrayList _loggers = new ArrayList();
        public void AddLogger(ILogger logger)
        {
            _loggers.Add(logger);
        }

        public void displayLog(string message)
        {
            foreach (ILogger item in _loggers)
            {
                item.WriteLog(message);
            }
        }

        
        public void displayLog(string buf, object a0)
        {
            var sb = new StringBuilder(buf.Length + 4);
            displayLog(sb.AppendFormat(buf, a0).ToString());
        }

        public void displayLog(string buf, object a0, object a1)
        {
            var sb = new StringBuilder(buf.Length + 8);
            displayLog(sb.AppendFormat(buf, a0, a1).ToString());
        }

        public void displayLog(string buf, object a0, object a1, object a2)
        {
            var sb = new StringBuilder(buf.Length + 12);
            displayLog(sb.AppendFormat(buf, a0, a1, a2).ToString());
        }

        public void displayLog(string buf, object a0, object a1, object a2, object a3)
        {
            var sb = new StringBuilder(buf.Length + 16);
            displayLog(sb.AppendFormat(buf, a0, a1, a2, a3).ToString());
        }

        public void displayLog(string buf, object[] args)
        {
            var sb = new StringBuilder(buf.Length + args.Length * 4);
            displayLog(buf);
        }
    }

    public interface ILogger
    {
        void WriteLog(string message);
    }
}


// Added to fix .NET MF bug
namespace System.Diagnostics
{
    public enum DebuggerBrowsableState
    {
        Never,
        Collapsed,
        RootHidden
    }

}