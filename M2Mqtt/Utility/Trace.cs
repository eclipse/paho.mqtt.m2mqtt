/*
Copyright (c) 2013, 2014 Paolo Patierno

All rights reserved. This program and the accompanying materials
are made available under the terms of the Eclipse Public License v1.0
and Eclipse Distribution License v1.0 which accompany this distribution. 

The Eclipse Public License is available at 
   http://www.eclipse.org/legal/epl-v10.html
and the Eclipse Distribution License is available at 
   http://www.eclipse.org/org/documents/edl-v10.php.

Contributors:
   Paolo Patierno - initial API and implementation and/or initial documentation
*/

using System.Diagnostics;

namespace uPLibrary.Networking.M2Mqtt.Utility
{
    /// <summary>
    /// Tracing levels
    /// </summary>
    public enum TraceLevel
    {
        /// <summary>
        /// Error
        /// </summary>
        Error = 0x01,
        /// <summary>
        /// Warning
        /// </summary>
        Warning = 0x02,
        /// <summary>
        /// Information
        /// </summary>
        Information = 0x04,
        /// <summary>
        /// Verbose
        /// </summary>
        Verbose = 0x0F,
        /// <summary>
        /// Frame
        /// </summary>
        Frame = 0x10,
        /// <summary>
        /// Queuing
        /// </summary>
        Queuing = 0x20
    }

    /// <summary>
    /// delegate for writing trace
    /// </summary>
    /// <param name="format">Format</param>
    /// <param name="args">Arg</param>
    public delegate void WriteTrace(string format, params object[] args);

    /// <summary>
    /// Tracing class
    /// </summary>
    public static class Trace
    {
        /// <summary>
        /// Trace Level
        /// </summary>
        public static TraceLevel TraceLevel;
        /// <summary>
        /// Write Trace
        /// </summary>
        public static WriteTrace TraceListener;

        /// <summary>
        /// Debug statement
        /// </summary>
        /// <param name="format">Format of the string</param>
        /// <param name="args">String arguments</param>
        [Conditional("DEBUG")]
        public static void Debug(string format, params object[] args)
        {
            if (TraceListener != null)
            {
                TraceListener(format, args);
            }
        }

        /// <summary>
        /// Writes a line to the console
        /// </summary>
        /// <param name="level">Trace level</param>
        /// <param name="format">Format of the string</param>
        public static void WriteLine(TraceLevel level, string format)
        {
            if (TraceListener != null && (level & TraceLevel) > 0)
            {
                TraceListener(format);
            }
        }

        /// <summary>
        /// Writes a line to the console
        /// </summary>
        /// <param name="level">Trace level</param>
        /// <param name="format">Format of the string</param>
        /// <param name="arg1">First argument</param>
        public static void WriteLine(TraceLevel level, string format, object arg1)
        {
            if (TraceListener != null && (level & TraceLevel) > 0)
            {
                TraceListener(format, arg1);
            }
        }

        /// <summary>
        /// Writes a line to the console
        /// </summary>
        /// <param name="level">Trace level</param>
        /// <param name="format">Format of the string</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        public static void WriteLine(TraceLevel level, string format, object arg1, object arg2)
        {
            if (TraceListener != null && (level & TraceLevel) > 0)
            {
                TraceListener(format, arg1, arg2);
            }
        }

        /// <summary>
        ///  Writes a line to the console
        /// </summary>
        /// <param name="level">Trace level</param>
        /// <param name="format">Format of the string</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <param name="arg3">Third argument</param>
        public static void WriteLine(TraceLevel level, string format, object arg1, object arg2, object arg3)
        {
            if (TraceListener != null && (level & TraceLevel) > 0)
            {
                TraceListener(format, arg1, arg2, arg3);
            }
        }
    }
}