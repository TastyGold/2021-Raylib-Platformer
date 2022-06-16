using Raylib_cs;
using System.Numerics;
using System.Collections.Generic;
using MathExtras;

namespace Engine
{
    public static class Rendering
    {
        public static int DrawCalls { get; private set; } = 0;
        private static readonly List<uint> batchIDs = new List<uint>();
        public static int Batches => batchIDs.Count;
        public static string GetBatchIDs()
        {
            string output = "";
            batchIDs.ForEach(n => output += n + ", ");
            return output;
        }

        private static uint lastTexCall = uint.MaxValue;
        public static void ResetDrawCalls()
        {
            DrawCalls = 0;
            batchIDs.Clear();
            lastTexCall = uint.MaxValue;
        }
        public static void CountDrawCall(uint textureID, int amount = 1)
        {
            if (textureID != lastTexCall)
            {
                batchIDs.Add(textureID);
                lastTexCall = textureID;
            }
            DrawCalls += amount;
        }
        public static void CountDrawCallSimple(int amount = 1)
        {
            batchIDs.Add(0);
            DrawCalls += amount;
        }

        [System.Obsolete("Use Clock.DeltaTime instead.")]
        public static float GetFrameTime()
        {
            return Raylib.GetFrameTime();
        }

        public static Rectangle GetScreenRect(Rectangle rect)
        {
            int u = Screen.scalar;
            return new Rectangle(rect.x * u, (-rect.height-rect.y) * u, rect.width * u, rect.height * u);
        }
        public static Rectangle GetScreenRect(float x, float y, float width, float height)
        {
            int u = Screen.scalar;
            return new Rectangle(x * u, (-height-y) * u, width * u, height * u);
        }
        public static Rectangle GetScreenRect(Vector2 position, Vector2 size)
        {
            return GetScreenRect(position.X, position.Y, size.X, size.Y);
        }
        public static int GetScreenPosition(float n)
        {
            return (int)(n * Screen.scalar);
        }
        public static Vector2Int GetScreenPosition(Vector2 position)
        {
            return new Vector2Int(GetScreenPosition(position.X), GetScreenPosition(position.Y));
        }

        public static Vector2 WorldVector(Vector2 position)
        {
            return position * Screen.scalar * new Vector2(1, -1);
        }
        public static Vector2 WorldVector(float x, float y)
        {
            return new Vector2(x, -y) * (Screen.pixelScale * Screen.pixelsPerUnit);
        }

        public static void DrawRectangleVx(Vectex vx)
        {
            int s = Screen.pixelsPerUnit / Screen.pixelScale;
            Raylib.DrawRectangle((int)(s*vx.min.X), (int)(s * -vx.max.Y), (int)(s*(vx.max.X - vx.min.X)), (int)(s*(vx.max.Y - vx.min.Y)), Color.WHITE);
            Rendering.CountDrawCallSimple();
        }

        public static Vector2 SpriteRectOffset => Vect.Zero;
    }
}