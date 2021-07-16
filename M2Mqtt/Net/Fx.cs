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
   .NET Foundation and Contributors - nanoFramework support
*/

using System.Threading;

namespace nanoFramework.M2Mqtt
{
    /// <summary>
    /// Support methods for specific framework
    /// </summary>
    public static class Fx
    {
        /// <summary>
        /// Starts the thread
        /// </summary>
        /// <param name="threadStart">Thread to start</param>
        public static void StartThread(ThreadStart threadStart) => new Thread(threadStart).Start();

        /// <summary>
        /// Puts the thread to sleep
        /// </summary>
        /// <param name="millisecondsTimeout">Timeout in Milliseconds</param>
        public static void SleepThread(int millisecondsTimeout) => Thread.Sleep(millisecondsTimeout);
    }
}
