using Raylib_cs;

namespace PGui
{
    public static class InputHelper
    {
        //Keyboard functions
        public static bool Held_LSHIFT => Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT);
        public static bool Held_LCTRL => Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL);
        public static bool Held_LALT => Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_ALT);

        //Mouse button functions
        public static bool Clicked_LMB => Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON);
        public static bool Held_LMB => Raylib.IsMouseButtonDown(MouseButton.MOUSE_LEFT_BUTTON);
        public static bool Released_LMB => Raylib.IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON);

        public static bool Clicked_RMB => Raylib.IsMouseButtonPressed(MouseButton.MOUSE_RIGHT_BUTTON);
        public static bool Held_RMB => Raylib.IsMouseButtonDown(MouseButton.MOUSE_RIGHT_BUTTON);
        public static bool Released_RMB => Raylib.IsMouseButtonReleased(MouseButton.MOUSE_RIGHT_BUTTON);

        public static bool Clicked_MB => Clicked_LMB || Clicked_RMB;
        public static bool Held_MB => Held_LMB || Held_RMB;
        public static bool Released_MB => Released_LMB || Released_RMB;
    }
}