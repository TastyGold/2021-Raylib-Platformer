using System.Numerics;
using Engine;
using Raylib_cs;
using MathExtras;
using System;

namespace Player
{
    public class Trail
    {
        private Random r = new Random();
        private Vector2[] points;
        private Color[] pointColors;
        private int startIndex = 0;
        public Color color;

        public void AddPoint(Vector2 newPoint)
        {
            points[startIndex] = newPoint;
            pointColors[startIndex] = color;
            startIndex += 1;
            if (startIndex == points.Length) startIndex = 0;
        }
        public void AddPoint(Vector2 newPoint, Color pointColor)
        {
            points[startIndex] = newPoint;
            pointColors[startIndex] = pointColor;
            startIndex += 1;
            if (startIndex == points.Length) startIndex = 0;
        }

        public void Draw()
        {
            for (int i = 0; i < points.Length - 1; i++)
            {
                int p1 = (i + startIndex) % points.Length;
                int p2 = (i + 1 + startIndex) % points.Length;

                Raylib.DrawLineV(points[p1] * Screen.scalar * Vect.FlipY, points[p2] * Screen.scalar * Vect.FlipY, pointColors[p2]);
            }
        }
        public Trail(int length, Color color)
        {
            points = new Vector2[length];
            pointColors = new Color[length];
            this.color = color;
        }
    }
}