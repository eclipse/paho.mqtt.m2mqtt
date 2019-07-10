#if (NANOFRAMEWORK_1_0)

namespace System
{
    class Environment
    {
        public static int TickCount
        {
            get { return (int)DateTime.UtcNow.Ticks; }
        }
    }
}

#endif
