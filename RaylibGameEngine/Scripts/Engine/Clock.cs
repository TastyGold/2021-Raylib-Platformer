using System.Diagnostics;
using System;

namespace Engine
{
    public static class Clock
    {
        private static readonly Stopwatch stopwatch = new Stopwatch();

        //Main clock
        private static long _gameTime = 0;
        public static long GameTime
        {
            get
            {
                Count();
                return _gameTime;
            }
        }
        public static void Count() => _gameTime = (long)((double)stopwatch.ElapsedMilliseconds * timeScale);
        public static void Start() => stopwatch.Start();
        public static void Restart() =>  stopwatch.Restart();

        //Timestamps
        public struct Timestamp
        {
            public long time;
            public Timestamp(long t)
            {
                time = t;
            }
        }        
        public static Timestamp Now => new Timestamp(_gameTime);
        public static float TimeSince(Timestamp t)
        {
            return TimeSinceMs(t) * 0.001f;
        }
        public static long TimeSinceMs(Timestamp t)
        {
            return _gameTime - t.time;
        }

        //Deltatime
        public static float timeScale = 1f;
        private static long lastElapsedMs = 0;
        public static float DeltaTime = 0;
        public static void AdvanceDeltaTime()
        {
            DeltaTime = GetFrameTime() * timeScale;
        }
        private static float GetFrameTime()
        {
            return 0.001f * GetFrameMs();
        }
        private static int GetFrameMs()
        {
            int n = (int)(stopwatch.ElapsedMilliseconds - lastElapsedMs);
            lastElapsedMs = stopwatch.ElapsedMilliseconds;
            return n;
        }
    }
}