using Raylib_cs;
using System.Numerics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Engine;
using MathExtras;
using System.Diagnostics;

namespace Levels
{
    public class Tilemap
    {
        //Tileset
        private readonly Texture2D tileset = ResourceTextures.grassTileset;
        private readonly int tileWidth = 8;
        private readonly int tileHeight = 6;
        private readonly Tiling.Format tilesetFormat = Tiling.Tileset8x6;
        private Vector2 GetTileRec(int index)
        {
            int x = index % tileWidth;
            int y = (index - x) / tileWidth;

            return new Vector2(x, y);
        }
        public Rectangle GetTilesetSourceRec(int index)
        {
            int tileX = index % tileWidth;
            int tileY = (index - tileX) / tileWidth;

            return new Rectangle(tileX * 16, tileY * 16, 16, 16);
        }

        //Tilemap
        private readonly byte?[,] tilemap = new byte?[512, 128];
        public byte?[,] GetTilemapData()
        {
            return tilemap;
        }
        public bool IsInBounds(int x, int y)
        {
            return !(x < 0 || x >= TilemapWidth || y < 0 || y >= TilemapHeight);
        }

        public Color GetTileColor(int ID) => ID switch
        {
            0 => Color.RED,
            1 => Color.BLUE,
            2 => Color.GREEN,
            3 => Color.YELLOW,
            _ => Color.WHITE,
        };

        public byte? GetTileData(int x, int y)
        {
            if (!IsInBounds(x, y))
            {
                return null;
            }
            return tilemap[x, y];
        }
        public byte? GetTileData(Vector2 pos)
        {
            int x = (int)pos.X;
            int y = (int)pos.Y;
            return GetTileData(x, y);
        }
        public byte?[,] GetTileDataRec(Vectex region)
        {
            return Get2DArrayRegion(tilemap, (int)region.min.X, (int)region.min.Y, (int)region.max.X, (int)region.max.Y);
        }
        public byte?[] GetTileColumn(int column)
        {
            byte?[] output = new byte?[tilemap.GetLength(1)];

            for (int y = 0; y < tilemap.GetLength(1); y++)
            {
                output[y] = tilemap[column, y];
            }

            return output;
        }
        public BitArray GetBitArray()
        {
            BitArray array = new BitArray(TilemapWidth * TilemapHeight);

            for (int x = 0; x < TilemapWidth; x++)
            {
                for (int y = 0; y < TilemapHeight; y++)
                {
                    array.Set(y + (x * TilemapHeight), tilemap[x, y].HasValue);
                }
            }

            return array;
        }
        public byte[] GetTilemapBitmask()
        {
            BitArray array = GetBitArray();

            byte[] bytes = new byte[array.Length / 8];
            array.CopyTo(bytes, 0);

            return bytes;
        }
        public byte[] GetTilemapDataBitmasked()
        {
            BitArray bitmask = GetBitArray();
            int cardinality = BlackMagic.GetCardinality(bitmask);
            byte[] tileIDs = new byte[cardinality];
            int i = 0;

            for (int x = 0; x < TilemapWidth; x++)
            {
                for (int y = 0; y < TilemapHeight; y++)
                {
                    if (bitmask.Get(y + (x * TilemapHeight)))
                    {
                        tileIDs[i] = (byte)tilemap[x, y];
                        i++;
                    }
                }
            }

            return tileIDs;
        }

        public static T[,] Get2DArrayRegion<T>(T[,] array, int ax, int ay, int bx, int by)
        {
            if (ax < 0 || bx < ax || ay < 0 || by < ay || bx >= array.GetLength(0) || by >= array.GetLength(1))
            {
                throw new IndexOutOfRangeException();
            }

            T[,] output = new T[bx - ax + 1, by - ay + 1];

            for (int x = ax, xi = 0; x <= bx; x++, xi++)
            {
                for (int y = ay, yi = 0; y <= by; y++, yi++)
                {
                    output[xi, yi] = array[x, y];
                }
            }

            return output;
        }

        public int TilemapWidth => tilemap.GetLength(0);
        public int TilemapHeight => tilemap.GetLength(1);

        public bool OverlapRec(float ax, float ay, float bx, float by)
        {
            if (ax > bx || ay > by)
            {
                throw new ArgumentException($"A must have values smaller than B: A = <{ax}, {ay}>, B = <{bx}, {by}>");
            }

            Vector2Int a = new Vector2Int((int)Math.Max(ax, 0), (int)Math.Max(ay, 0));
            Vector2Int b = new Vector2Int((int)Math.Min(bx, TilemapWidth - 1), (int)Math.Min(by, TilemapHeight - 1));

            for (int x = a.X; x <= b.X; x++)
            {
                for (int y = a.Y; y <= b.Y; y++)
                {
                    if (tilemap[x, y].HasValue)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        public bool OverlapRec(Vectex v)
        {
            return OverlapRec(v.min.X, v.min.Y, v.max.X, v.min.Y);
        }
        public bool OverlapHitbox(Vector2 position, Vector2 hitboxSize)
        {
            return OverlapRec(position.X - (hitboxSize.X / 2), position.Y - (hitboxSize.X / 2), position.X + (hitboxSize.X / 2), position.Y + (hitboxSize.X / 2));
        }

        //Tilemap manipulation
        public void SetTile(int x, int y, byte? id = 9)
        {
            if (IsInBounds(x, y))
            tilemap[x, y] = id;
        }
        public void SetTile(Vector2 coords, byte? id = 9)
        {
            SetTile((int)coords.X, (int)coords.Y, id);
        }
        public void SetTilesRec(int ax, int ay, int bx, int by, byte? id = 9)
        {
            int startX = Math.Min(ax, bx);
            int startY = Math.Min(ay, by);
            int endX = Math.Max(ax, bx);
            int endY = Math.Max(ay, by);

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    SetTile(x, y, id);
                }
            }
        }
        public void SetTilesRec(Vector2 startPos, Vector2 endPos, byte? id = 9)
        {
            SetTilesRec((int)startPos.X, (int)startPos.Y, (int)endPos.X, (int)endPos.Y, id);
        }
        public void FormatTile(int x, int y)
        {
            if (IsInBounds(x, y) && tilemap[x, y].HasValue)
            {
                //OldFormat(x, y);
                tilemap[x, y] = (byte?)BetterTiling.GetTileID(BetterTiling.GetTilingCode(tilemap, x, y), BetterTiling.TileRuleset.Tileset8x6);
            }
        }
        public void FormatTile(Vector2 tile)
        {
            FormatTile((int)tile.X, (int)tile.Y);
        }
        public void FormatTilesRec(int ax, int ay, int bx, int by)
        {
            int startX = Math.Min(ax, bx);
            int startY = Math.Min(ay, by);
            int endX = Math.Max(ax, bx);
            int endY = Math.Max(ay, by);

            startX = startX < 0 ? 0 : startX;
            startY = startY < 0 ? 0 : startY;
            endX = endX >= TilemapWidth ? TilemapWidth - 1 : endX;
            endY = endY >= TilemapHeight ? TilemapHeight - 1: endY;

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    FormatTile(x, y);
                }
            }
        }
        public void FormatTilesRec(Vector2 startPos, Vector2 endPos)
        {
            FormatTilesRec((int)startPos.X, (int)startPos.Y, (int)endPos.X, (int)endPos.Y);
        }

        public void OldFormat(int x, int y)
        {
            byte? tileIndex = null;
            string code = Tiling.GetTilingCode(tilemap, x, y);
            for (byte ry = 0; ry < tileHeight; ry++)
            {
                for (byte rx = 0; rx < tileWidth; rx++)
                {
                    if (tilesetFormat.rules[rx, ry].CompareCode(code))
                    {
                        tileIndex = (byte)((ry * tileWidth) + rx);
                        ry = (byte)tileHeight;
                        rx = (byte)tileWidth;
                    }
                }
            }
            if (tileIndex != null)
            {
                tilemap[x, y] = tileIndex;
            }
            else
            {
                throw new Exception($"Failed to format tile at <{x}, {y}> " +
                    $"\nAttempted formatting code = {code}");
            }
        }

        //Rendering
        public void DrawTile(int index, int x, int y)
        {
            Raylib.DrawTexturePro(
                tileset,
                GetTilesetSourceRec(index),
                Rendering.GetScreenRect(new Rectangle(x, y, 1, 1)),
                new Vector2(0, 0),
                0f,
                Color.WHITE
                );
            Rendering.CountDrawCall(tileset.id);
        }
        public void DrawTilesRec(int startX, int startY, int endX, int endY)
        {
            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    if (tilemap[x, y].HasValue)
                    {
                        int u = Screen.scalar;
                        Raylib.DrawTexturePro(
                            tileset,
                            GetTilesetSourceRec((byte)tilemap[x, y]),
                            new Rectangle(x * u, (-y - 1) * u, u, u),
                            new Vector2(0, 0),
                            0f,
                            Color.WHITE
                            );
                        Rendering.CountDrawCall(tileset.id);
                    }
                }
            }
        }
        public void DrawAllTiles()
        {
            for (int x = 0; x < TilemapWidth; x++)
            {
                for (int y = 0; y < TilemapHeight; y++)
                {
                    if (tilemap[x, y].HasValue)
                    {
                        int u = Screen.scalar;
                        Raylib.DrawTexturePro(
                            tileset,
                            GetTilesetSourceRec((byte)tilemap[x, y]),
                            new Rectangle(x * u, (-y - 1) * u, u, u),
                            new Vector2(0, 0),
                            0f,
                            Color.WHITE
                            );
                        Rendering.CountDrawCall(tileset.id);
                    }
                }
            }
        }

        public RenderTexture2D BakeTilemapTexture()
        {
            Stopwatch t = new Stopwatch();
            t.Start();

            RenderTexture2D tex = Raylib.LoadRenderTexture(TilemapWidth * 16, TilemapHeight * 16);
            Raylib.BeginTextureMode(tex);
            for (int x = 0; x < TilemapWidth; x++)
            {
                for (int y = 0; y < TilemapHeight; y++)
                {
                    if (tilemap[x, y].HasValue)
                    {
                        Rectangle srec = GetTilesetSourceRec((byte)tilemap[x, y]);
                        srec.height *= -1;

                        Raylib.DrawTextureRec(
                            tileset,
                            srec,
                            new Vector2(x*16, y*16),
                            Color.WHITE
                            );
                    }
                }
            }
            Raylib.EndTextureMode();

            Console.WriteLine("Tilemap texture baked: " + t.ElapsedMilliseconds + "ms");
            return tex;
        }
        public void DrawTilemapToTexture()
        {
            for (int x = 0; x < TilemapWidth; x++)
            {
                for (int y = 0; y < TilemapHeight; y++)
                {
                    if (tilemap[x, y].HasValue)
                    {
                        Rectangle srec = GetTilesetSourceRec((byte)tilemap[x, y]);
                        srec.height *= -1;

                        Raylib.DrawTextureRec(
                            tileset,
                            srec,
                            new Vector2(x * 16, y * 16),
                            Color.WHITE
                            );
                    }
                }
            }
        }

        private const float gridZoomOutPoint = 0.2f;
        private const int largeGridScaleAmount = 8;
        private Color gridColor = new Color(100, 100, 100, 100);
        public void DrawGrid(float cameraZoom)
        {
            int s = (int)Screen.scalar;
            int increment = cameraZoom > gridZoomOutPoint ? 1 : largeGridScaleAmount;

            for (int x = 0; x <= TilemapWidth; x += increment)
            {
                Raylib.DrawLine(x * s, 0, x * s, -TilemapHeight * s, gridColor);
                Rendering.CountDrawCall(tileset.id);
            }
            for (int y = 0; y <= TilemapHeight; y += increment)
            {
                Raylib.DrawLine(0, -y * s, TilemapWidth * s, -y * s, gridColor);
                Rendering.CountDrawCall(tileset.id);
            }
        }
        public void DrawGrid(int increment, Color gridColor)
        {
            int s = (int)Screen.scalar;

            for (int x = 0; x <= TilemapWidth; x += increment)
            {
                Raylib.DrawLine(x * s, 0, x * s, -TilemapHeight * s, gridColor);
                Rendering.CountDrawCall(tileset.id);
            }
            for (int y = 0; y <= TilemapHeight; y += increment)
            {
                Raylib.DrawLine(0, -y * s, TilemapWidth * s, -y * s, gridColor);
                Rendering.CountDrawCall(tileset.id);
            }
        }

        //Initialisation
        public Tilemap()
        {

        }
        public Tilemap(byte?[,] map)
        {
            tilemap = map;
        }
    }
}