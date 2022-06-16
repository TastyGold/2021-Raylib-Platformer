using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;
using MathExtras;
using Engine;

namespace Levels
{
    public class DecorObject
    {
        public readonly byte ThemeIndex;
        public readonly byte AtlasTextureID;

        private AtlasTexture2D tex;

        private Vector2Int position;
        private readonly PixelPosition offset;

        public void DrawToTexture()
        {
            Rectangle srec = tex.SourceRec;
            srec.height *= -1;
            Raylib.DrawTextureRec(tex.texture, srec, ((Vector2)position*16) + new Vector2(offset.X, offset.Y), Color.WHITE);
        }

        public DecorObject(AtlasTexture2D tex, Vector2Int position, PixelPosition offset)
        {
            this.tex = tex;
            this.position = position;
            this.offset = offset;
        }
    }
}