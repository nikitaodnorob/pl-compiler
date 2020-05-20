namespace MyCompilerLibrary
{
    public static class Performance
    {
        readonly static System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        public static void start() => sw.Start();

        public static void restart() => sw.Restart();

        public static void reset() => sw.Reset();

        public static void stop() => sw.Stop();

        public static long milliseconds { get => sw.ElapsedMilliseconds; }

    }
}
