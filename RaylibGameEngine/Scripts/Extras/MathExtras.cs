using System;
using System.Numerics;
using System.Collections;

namespace MathExtras
{
    /// <summary>
    /// Static class containing extension methods and constants involving Vectors
    /// </summary>
    public static class Vect
    {
        public static Vector2 Zero => new Vector2(0, 0);
        public static Vector2 Up => new Vector2(0, 1);
        public static Vector2 Down => new Vector2(0, -1);
        public static Vector2 Left => new Vector2(-1, 0);
        public static Vector2 Right => new Vector2(1, 0);
        public static Vector2 FlipY => new Vector2(1, -1);
        public static Vector2 FlipX => new Vector2(-1, 1);

        public static Vector2 Floor(this Vector2 vector)
        {
            return new Vector2((float)Math.Floor(vector.X), (float)Math.Floor(vector.Y));
        }
        public static Vector2 Ceiling(this Vector2 vector)
        {
            return new Vector2((float)Math.Ceiling(vector.X), (float)Math.Ceiling(vector.Y));
        }
        public static Vector2 RoundXY(this Vector2 vector)
        {
            return new Vector2(
                vector.X % 1 < 0.5f ? (int)vector.X : (int)vector.X + 1,
                vector.Y % 1 < 0.5f ? (int)vector.Y : (int)vector.Y + 1
                );
        }
        public static Vector2 Normalise(this Vector2 vector)
        {
            if (vector.X != 0 && vector.Y != 0)
            {
                return vector / vector.Length();
            }
            else
            {
                return vector;
            }
        }
        public static Vector2Int ToVector2Int(this Vector2 vector)
        {
            return new Vector2Int((int)vector.X, (int)vector.Y);
        }

        public static string ToString(this Vector2 vector, int decimalPlaces)
        {
            return $"<{Math.Round(vector.X, decimalPlaces)}, {Math.Round(vector.Y, decimalPlaces)}>";
        }
    }

    /// <summary>
    /// Defines a rectangle represented by two Vector2 values for the bottom left and top right corners.
    /// </summary>
    public struct Vectex
    {
        //Variables
        public Vector2 min;
        public Vector2 max;

        //Initialisation
        public Vectex(Vector2 a, Vector2 b)
        {
            min = new Vector2(a.X < b.X ? a.X : b.X, a.Y < b.Y ? a.Y : b.Y);
            max = new Vector2(a.X >= b.X ? a.X : b.X, a.Y >= b.Y ? a.Y : b.Y);
        }
        public Vectex(float ax, float ay, float bx, float by)
        {
            min = new Vector2(ax < bx ? ax : bx, ay < by ? ay : by);
            max = new Vector2(ax >= bx ? ax : bx, ay >= by ? ay : by);
        }

        //Functions
        public Vector2 Size => new Vector2(max.X - min.X, max.Y - min.Y);

        //Methods
        public Vectex SwapY() => new Vectex()
        {
            min = new Vector2(min.X, max.Y),
            max = new Vector2(max.X, min.Y)
        };
        public Vector2 RandomPositionInside(Random rand)
        {
            return new Vector2(min.X + (0.1f * rand.Next(0, (int)(10 * (max.X - min.X)))), min.Y + (0.1f * rand.Next(0, (int)(10 * (max.Y - min.Y)))));
        }

        //Overrides
        public override string ToString()
        {
            return $"{min.ToString()}, {max.ToString()}";
        }
    }

    /// <summary>
    /// Simple struct containing functions for 2D ellipses.
    /// </summary>
    public struct Ellipse
    {
        public Vector2 center;
        public Vector2 radii;

        public bool EnclosesPoint(Vector2 point)
        {
            float px = point.X - center.X;
            float py = point.Y - center.Y;

            float pxsq = px * px;
            float pysq = py * py;

            float rxsq = radii.X * radii.X;
            float rysq = radii.Y * radii.Y;

            return (pxsq / rxsq) + (pysq / rysq) <= 0.25f;
        }
    }

    /// <summary>
    /// 2D Vector with 32-bit integer precision
    /// </summary>
    public struct Vector2Int
    {
        //Variables
        public int X;
        public int Y;

        //Initilisation
        public Vector2Int(int x, int y)
        {
            X = x;
            Y = y;
        }
        public Vector2Int(Vector2 vector)
        {
            X = (int)vector.X;
            Y = (int)vector.Y;
        }

        //Constants
        public static Vector2Int Zero => new Vector2Int(0, 0);

        //Math
        public int LengthSquared => (X * X) + (Y * Y);
        public float Length => (float)Math.Sqrt(LengthSquared);

        //Operators
        public static Vector2Int operator +(Vector2Int a, Vector2Int b) => new Vector2Int(a.X + b.X, a.Y + b.Y);
        public static Vector2Int operator -(Vector2Int a, Vector2Int b) => new Vector2Int(a.X - b.X, a.Y - b.Y);
        public static Vector2Int operator *(Vector2Int a, Vector2Int b) => new Vector2Int(a.X * b.X, a.Y * b.Y);
        public static Vector2Int operator /(Vector2Int a, Vector2Int b) => new Vector2Int(a.X / b.X, a.Y / b.Y);
        public static explicit operator Vector2Int(Vector2 v)
        {
            return new Vector2Int((int)v.X, (int)v.Y);
        } 
        public static explicit operator Vector2(Vector2Int v)
        {
            return new Vector2(v.X, v.Y);
        }

        //Overrides
        public override string ToString()
        {
            return $"<{X.ToString()}, {Y.ToString()}>";
        }
        public override bool Equals(object obj)
        {
            if (obj is Vector2Int)
            {
                return ((Vector2Int)obj).X == X && ((Vector2Int)obj).Y == Y;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    /// <summary>
    /// Simple small Vector2Int struct to represent a pixel within a 16x16 tile, represented by one byte.
    /// </summary>
    public struct PixelPosition
    {
        public const float pixelSize = 1 / 16;

        //Data
        public byte Value { get; private set; }

        //Properties
        public byte X
        {
            get => (byte)(Value >> 4);
            set
            {
                Value &= 0xf;
                Value += (byte)(value << 4);
            }
        }
        public byte Y
        {
            get => (byte)(Value & 0xf);
            set
            {
                Value &= 0xf0;
                Value += (byte)(value & 0xf);
            }
        }

        public Vector2 GetVector2() => new Vector2(pixelSize * X, pixelSize * Y);

        //Constructors
        public PixelPosition(int x, int y)
        {
            Value = 0;
            X = (byte)x;
            Y = (byte)y;
        }
        public PixelPosition(byte value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// Static class containing various functions to check for overlaps between objects.
    /// </summary>
    public static class Intersection
    {
        public static bool AABB(Vector2 positionA, Vector2 sizeA, Vector2 positionB, Vector2 sizeB)
        {
            return Math.Abs(positionB.X - positionA.X) < (sizeA.X + sizeB.X)
                && Math.Abs(positionB.Y - positionA.Y) < (sizeA.Y + sizeB.Y);
        }
        public static bool AABB(Vectex a, Vectex b)
        {
            return !(a.min.X > b.max.X || a.max.X < b.min.X || a.max.Y < b.min.Y || a.min.Y > b.min.Y);
        }
        public static bool OverlapTile(Vector2 position, Vector2 hitboxSize, Vector2Int tile)
        {
            return OverlapTileX(position, hitboxSize, tile) && OverlapTileY(position, hitboxSize, tile);
        }
        public static bool OverlapTileX(Vector2 position, Vector2 hitboxSize, Vector2Int tile)
        {
            return Math.Abs(position.X - tile.X) < (hitboxSize.X / 2) + 0.5f;
        }
        public static bool OverlapTileY(Vector2 position, Vector2 hitboxSize, Vector2Int tile)
        {
            return Math.Abs(position.Y - tile.Y) < (hitboxSize.Y / 2) + 0.5f;
        }
    }

    public static class MathP
    {
        /// <summary>
        /// Linearly interpolates between two values.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="t">eg: 0 = a , 1 = b , 0.5 = ab/2</param>
        /// <returns></returns>
        public static float Lerp(float a, float b, float t)
        {
            return a + ((b - a) * t);
        }
        public static float InvLerp(float a, float b, float t)
        {
            return (t - a) / (b - a);
        }
        /// <summary>
        /// Returns value a interpolated towards value b, with a maximum step value.
        /// Result will not exceed value b.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="maxStep"></param>
        /// <returns></returns>
        public static float StepTowards(float a, float b, float maxStep)
        {
            if (Math.Abs(b - a) < maxStep || a == b)
            {
                return b;
            }
            else
            {
                return a + (maxStep * (b > a ? 1 : -1));
            }
        }
        /// <summary>
        /// Simplified division for simple numbers, returns whole number (rounds up)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public static int CountBack(this float value, float step)
        {
            int steps = 0;
            while (value > step)
            {
                value -= step;
                steps++;
            }
            return steps;
        }

        public enum Sign
        {
            Positive, Negative, Zero
        }
        public static Sign GetSign(this float f)
        {
            if (f == 0) return Sign.Zero;
            return f < 0 ? Sign.Negative : Sign.Positive;
        }
        public static bool HasSimilarSign(this Sign s, Sign c)
        {
            return c == Sign.Zero || s == Sign.Zero || c == s;
        }

        public static Raylib_cs.Color ColorLerp(Raylib_cs.Color a, Raylib_cs.Color b, float t)
        {
            return new Raylib_cs.Color((int)Lerp(a.r, b.r, t), (int)Lerp(a.g, b.g, t), (int)Lerp(a.b, b.b, t), (int)Lerp(a.a, b.a, t));
        }
    }

    public static class BlackMagic
    {
        public static Int32 GetCardinality(BitArray bitArray)
        {
            int[] ints = new int[(bitArray.Count >> 5) + 1];

            bitArray.CopyTo(ints, 0);

            int count = 0;

            // fix for not truncated bits in last integer that may have been set to true with SetAll()
            ints[^1] &= ~(-1 << (bitArray.Count % 32));

            for (int i = 0; i < ints.Length; i++)
            {

                int c = ints[i];

                // magic (http://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel)
                unchecked
                {
                    c = c - ((c >> 1) & 0x55555555);
                    c = (c & 0x33333333) + ((c >> 2) & 0x33333333);
                    c = ((c + (c >> 4) & 0xF0F0F0F) * 0x1010101) >> 24;
                }

                count += c;

            }

            return count;

        }
    }
}