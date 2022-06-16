using System;
using System.Collections.Generic;
using System.Numerics;
using MathExtras;
using Engine;
using Raylib_cs;

namespace Levels
{
    public struct Semisolid : EditorPlus.IDraggable
    {
        //Static const variables
        public const float colliderTrimming = 0.2f;
        public const float surfaceDepthLeniency = 0.2f;

        //Meta variables
        public byte themeIndex;
        public SpriteSheet tileSheet;

        //Struct variables
        public int x;
        public int y;
        public int width;
        public int height;
        public bool hasSurface;

        public CullingRule culling;

        //Corner members
        public Vector2Int CornerA
        {
            get
            {
                return new Vector2Int(x, y);
            }
            set
            {
                if (value.X - x >= width || value.Y - y >= height)
                {
                    throw new ArgumentOutOfRangeException();
                }
                width -= (ushort)(value.X - x);
                height -= (ushort)(value.Y - y);
                x = (ushort)value.X;
                y = (ushort)value.Y;
            }
        }
        public Vector2Int CornerB
        {
            get
            {
                return new Vector2Int(x + width - 1, y + height - 1);
            }
            set
            {
                if (value.X - x <= 0 || value.Y <= 0)
                {
                    throw new ArgumentOutOfRangeException();
                }
                width = (ushort)(value.X - x);
                height = (ushort)(value.Y - y);
            }
        }
        public Vector2Int CornerC => new Vector2Int(x, y + height - 1);
        public Vector2Int CornerD => new Vector2Int(x + width - 1, y);

        //Size and dimensions members
        public int StandingAreaWidth => width - (culling.left ? 0 : 1) - (culling.right ? 0 : 1);
        public int EquatorialAreaHeight => height - (culling.top ? 0 : 1) - (culling.bottom ? 0 : 1);
        public ushort SurfaceHeight => (ushort)(y + height - 1);
        public Rectangle GetRect() => new Rectangle(x, y, width, height);
        public bool Overlaps(Vector2Int coord)
        {
            return x <= coord.X && y <= coord.Y && coord.X < x + width && coord.Y < y + height;
        }

        //Rendering
        public void DrawSmart()
        {
            Vector2Int centerCornerA = CornerA + new Vector2Int(culling.left ? 0 : 1, culling.bottom ? 0 : 1);
            Vector2Int centerCornerB = CornerB - new Vector2Int(culling.right ? 0 : 1, culling.top ? 0 : 1);

            bool hasStandingArea = centerCornerA.X <= centerCornerB.X;
            bool hasEquatioralArea = centerCornerA.Y <= centerCornerB.Y;

            //Draw top
            if (!culling.top)
            {
                if (!culling.left) DrawCornerToScreen(2);
                if (!culling.right) DrawCornerToScreen(3);
                if (hasStandingArea) DrawSectionToScreen(1, hasStandingArea ? 1 : 0, centerCornerA.X, CornerB.Y, StandingAreaWidth, 1);
            }

            //Draw middle layers
            if (hasEquatioralArea)
            {
                if (!culling.left) DrawSectionToScreen(0, 2, x, centerCornerB.Y - EquatorialAreaHeight + 1, 1, EquatorialAreaHeight);
                if (!culling.right) DrawSectionToScreen(2, 2, CornerB.X, centerCornerB.Y - EquatorialAreaHeight + 1, 1, EquatorialAreaHeight);
                if (hasStandingArea) DrawSectionToScreen(1, 2, centerCornerA.X, centerCornerB.Y - EquatorialAreaHeight + 1,StandingAreaWidth, EquatorialAreaHeight);
            }

            //Draw bottom
            if (!culling.bottom)
            {
                if (!culling.left) DrawCornerToScreen(0);
                if (!culling.right) DrawCornerToScreen(1);
                if (hasStandingArea) DrawSectionToScreen(1, 3, centerCornerA.X, y, StandingAreaWidth, 1);
            }
        }
        /// <summary>
        /// Draws one of the four corners
        /// </summary>
        /// <param name="corner">0 - Bottom left, 1 - Bottom right, 2 - Top left, 3 - Top right</param>
        private void DrawCornerToScreen(int corner)
        {
            Rectangle srec = corner switch
            {
                0 => tileSheet.GetSourceRec(0, 3),
                1 => tileSheet.GetSourceRec(2, 3),
                2 => tileSheet.GetSourceRec(0, hasSurface ? 1 : 0),
                3 => tileSheet.GetSourceRec(2, hasSurface ? 1 : 0),
                _ => throw new ArgumentOutOfRangeException($"Corner {corner} is invalid")
            };
            Vector2 destRec = (Vector2)(corner switch
            {
                0 => CornerA,
                1 => CornerD,
                2 => CornerC,
                3 => CornerB,
                _ => throw new ArgumentOutOfRangeException($"Corner {corner} is invalid")
            });

            Raylib.DrawTexturePro(
                tileSheet.texture,
                srec,
                Rendering.GetScreenRect(destRec.X, destRec.Y, 1, 1),
                Rendering.SpriteRectOffset, 0, Color.WHITE
                );
            Rendering.CountDrawCall(tileSheet.texture.id);
        }
        private void DrawSectionToScreen(int srecX, int srecY, int posX, int posY, int width, int height)
        {
            Raylib.DrawTextureTiled(
                tileSheet.texture,
                tileSheet.GetSourceRec(srecX, srecY),
                Rendering.GetScreenRect(posX, posY, width, height),
                Rendering.SpriteRectOffset, 0, Screen.pixelScale, Color.WHITE
                );
            Rendering.CountDrawCall(tileSheet.texture.id);
        }

        public void DrawToTexture()
        {
            //Inefficient algorithm for realtime drawing
            //Each tile is drawn indivually, because drawTextureTiled causes issues 

            for (int xi = 0; xi < width; xi++)
            {
                for (int yi = 0; yi < height; yi++)
                {
                    //Tilesheet texture selection based on position
                    int sx = (culling.left ? false : xi == 0) ? 0 : ((culling.right ? false : xi == width - 1) ? 2 : 1);
                    int sy = (culling.bottom ? false : yi == 0) ? 3 : ((culling.top ? false : yi == height - 1) ? (hasSurface ? 1 : 0) : 2);

                    Rectangle sourceRec = tileSheet.GetSourceRec(sx, sy);
                    sourceRec.height *= -1;

                    Raylib.DrawTextureRec(tileSheet.texture, sourceRec, new Vector2(x + xi, y + yi)*16, Color.WHITE);
                }
            }
        }

        //Transformation
        public void DragPosition(Vector2Int vector)
        {
            x += vector.X;
            y += vector.Y;
        }

        //Initialisation
        public Semisolid(int positionX, int positionY, int width, int height, bool hasSurface, byte themeIndex)
        {
            this.themeIndex = (byte)themeIndex;
            tileSheet = new SpriteSheet(16, ResourceTextures.grassSemisolidTileset);
            culling = new CullingRule(false, false, false, false);

            x = (ushort)positionX;
            this.width = (ushort)width;
            this.height = (ushort)height;
            this.hasSurface = hasSurface;
            y = (ushort)positionY;
        }
        public Semisolid(Vector2 CornerA, Vector2 CornerB, bool hasSurface, byte themeIndex)
        {
            Vectex v = new Vectex(CornerA, CornerB);

            v.min = v.min.Floor();
            v.max = v.max.Ceiling() + Vector2.One;

            this.themeIndex = themeIndex;
            tileSheet = new SpriteSheet(16, ResourceTextures.grassSemisolidTileset);
            culling = new CullingRule(false, false, false, false);

            x = (ushort)v.min.X;
            width = (ushort)v.max.X - (ushort)v.min.X;
            height = (ushort)v.max.Y - (ushort)v.min.Y;
            this.hasSurface = hasSurface;
            y = (ushort)v.min.Y;
        }

        //Culling rule struct for semisolid edges and corners
        public struct CullingRule
        {
            public bool top;
            public bool bottom;
            public bool left;
            public bool right;

            public bool All
            {
                get
                {
                    return top && bottom && left && right;
                }
                set
                {
                    top = value;
                    bottom = value;
                    left = value;
                    right = value;
                }
            }

            public CullingRule(bool top, bool bottom, bool left, bool right)
            {
                this.top = top;
                this.bottom = bottom;
                this.left = left;
                this.right = right;
            }
        }
    }

    public static class SemisolidFunc
    {
        public static void DrawAll(this List<Semisolid> ss)
        {
            foreach (Semisolid s in ss)
            {
                s.DrawSmart();
            }
        }

        public static List<Semisolid> GetSemisolidOverlaps(this List<Semisolid> ss, Vectex area)
        {
            List<Semisolid> ns = new List<Semisolid>();

            foreach (Semisolid s in ss)
            {
                if (s.x + s.width > area.min.X && s.x < area.max.X)
                    if (s.y + s.height > area.min.Y && s.y < area.max.Y)
                    {
                        ns.Add(s);
                    }
            }

            return ns;
        }
    }
}