using System;
using System.Numerics;
using Raylib_cs;
using MathExtras;

namespace Engine
{
    public static class Input
    {
        public static Vector2 GetWASDInput()
        {
            Vector2 inputVector = Vector2.Zero;

            if (Raylib.IsKeyDown(KeyboardKey.KEY_W))
            {
                inputVector += Vect.Up;
            }

            if (Raylib.IsKeyDown(KeyboardKey.KEY_A))
            {
                inputVector += Vect.Left;
            }

            if (Raylib.IsKeyDown(KeyboardKey.KEY_S))
            {
                inputVector += Vect.Down;
            }

            if (Raylib.IsKeyDown(KeyboardKey.KEY_D))
            {
                inputVector += Vect.Right;
            }

            return inputVector;
        }
        public static Vector2 GetWASDInputNormalised()
        {
            return GetWASDInput().Normalise();
        }
        public static Vector2 GetArrowInput()
        {
            Vector2 inputVector = Vector2.Zero;

            if (Raylib.IsKeyDown(KeyboardKey.KEY_UP))
            {
                inputVector += Vect.Up;
            }

            if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT))
            {
                inputVector += Vect.Left;
            }

            if (Raylib.IsKeyDown(KeyboardKey.KEY_DOWN))
            {
                inputVector += Vect.Down;
            }

            if (Raylib.IsKeyDown(KeyboardKey.KEY_RIGHT))
            {
                inputVector += Vect.Right;
            }

            return inputVector;
        }

        public static bool ShiftDown() => Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT);
        public static bool CtrlDown() => Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL);

        public static float GetMouseScroll()
        {
            return Raylib.GetMouseWheelMove();
        }
        
        public static MouseButton GetMouseButton(int button)
        {
            return button switch
            {
                0 => MouseButton.MOUSE_LEFT_BUTTON,
                1 => MouseButton.MOUSE_RIGHT_BUTTON,
                2 => MouseButton.MOUSE_MIDDLE_BUTTON,
                _ => throw new IndexOutOfRangeException(),
            };
        }
        public static bool MouseButtonPressed(int button) => Raylib.IsMouseButtonPressed(GetMouseButton(button));
        public static bool MouseButtonReleased(int button) => Raylib.IsMouseButtonReleased(GetMouseButton(button));
        public static bool MouseButtonDown(int button) => Raylib.IsMouseButtonDown(GetMouseButton(button));
        public static bool MouseButtonUp(int button) => Raylib.IsMouseButtonUp(GetMouseButton(button));
    }
}