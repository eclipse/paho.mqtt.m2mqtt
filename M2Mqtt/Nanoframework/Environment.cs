/*
Contributors:   
   .NET Foundation and Contributors - nanoFramework support
*/

namespace System
{
    static class Environment
    {
        public static int TickCount => (int)DateTime.UtcNow.Ticks;
    }
}
