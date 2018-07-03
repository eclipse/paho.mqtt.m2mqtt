using System;

namespace System
{
    class Environment
    {
        public static int TickCount
        {
            get { return (int)new DateTime().Ticks; }
        }
    }
}
