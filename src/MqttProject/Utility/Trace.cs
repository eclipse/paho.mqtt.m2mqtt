using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Diagnostics;

namespace uPLibrary.Networking.M2Mqtt.Utility
{
    /// <summary>
    /// Tracing levels
    /// </summary>
    public enum TraceLevel
    {
        Error = 0x01,
        Warning = 0x02,
        Information = 0x04,
        Verbose = 0x0F,
        Frame = 0x10,
        Queuing = 0x20
    }

    // delegate for writing trace
    public delegate void WriteTrace(string format, params object[] args);

    /// <summary>
    /// Tracing class
    /// </summary>
    public static class Trace
    {
        public static TraceLevel TraceLevel;
        public static WriteTrace TraceListener;

        [Conditional("DEBUG")]
        public static void Debug(string format, params object[] args)
        {
            if (TraceListener != null)
            {
                TraceListener(format, args);
            }
        }

        public static void WriteLine(TraceLevel level, string format)
        {
            if (TraceListener != null && (level & TraceLevel) > 0)
            {
                TraceListener(format);
            }
        }

        public static void WriteLine(TraceLevel level, string format, object arg1)
        {
            if (TraceListener != null && (level & TraceLevel) > 0)
            {
                TraceListener(format, arg1);
            }
        }

        public static void WriteLine(TraceLevel level, string format, object arg1, object arg2)
        {
            if (TraceListener != null && (level & TraceLevel) > 0)
            {
                TraceListener(format, arg1, arg2);
            }
        }

        public static void WriteLine(TraceLevel level, string format, object arg1, object arg2, object arg3)
        {
            if (TraceListener != null && (level & TraceLevel) > 0)
            {
                TraceListener(format, arg1, arg2, arg3);
            }
        }
    }
}