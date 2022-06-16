using System;
using System.Text;
using AnimJson;
using System.Numerics;

namespace Engine
{
    public static class EntryPoint
    {
        public enum LaunchMode
        {
            Editor,
            Gameplay
        }

        public static LaunchMode startupMode = LaunchMode.Gameplay;
        public static string levelPath = "NewTestLevel.lvl";

        public static void Main()
        {
            switch (startupMode)
            {
                case LaunchMode.Editor:
                    EditorPlus.Launch();
                    break;
                case LaunchMode.Gameplay:
                    Gameplay.Launch();
                    break;
            }
        }
    }
}