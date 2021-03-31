#if (NANOFRAMEWORK_1_0)

namespace System
{
    static class Environment
    {
        public static int TickCount
        {
            get { return (int)DateTime.UtcNow.Ticks; }
        }
    }
}

#endif
