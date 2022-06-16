using System;
using System.Numerics;
using Raylib_cs;
using MathExtras;

namespace Engine
{
    public static class Screen
    {
        //Screen setup data
        public static int screenWidth = 400 * pixelScale;
        public static int screenHeight = 225 * pixelScale;

        public const int pixelsPerUnit = 16;
        public const float pxl = (float)1 / pixelsPerUnit;
        public const int pixelScale = 4;
        public const int scalar = pixelsPerUnit * pixelScale;

        public static Vector2 GetMouseWorldPosition(this Camera2D cam)
        {
            Vector2 mousePos = Raylib.GetMousePosition();
            mousePos.Y = screenHeight - mousePos.Y;
            mousePos -= cam.offset;

            Vector2 pos = cam.target / scalar * Vect.FlipY;
            pos += mousePos / scalar / cam.zoom;

            return pos;
        }
    }
}