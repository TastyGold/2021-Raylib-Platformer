using Raylib_cs;
using System;
using System.Collections.Generic;
using MathExtras;
using System.Numerics;
using Levels;
using System.IO;
using System.Resources;

namespace Engine
{
    public static class ResourceTextures
    {
        #region References
        public static Texture2D grassTileset = LoadTextureFromResource(RaylibGameEngine.Properties.Resources.templetileset);
        public static Texture2D grassSemisolidTileset = LoadTextureFromResource(RaylibGameEngine.Properties.Resources.templetilesetsemisolid);
        public static Texture2D crystalTileset = LoadTextureFromResource(RaylibGameEngine.Properties.Resources.rockyTileset);
        public static Texture2D crystalSemisolidTileset = LoadTextureFromResource(RaylibGameEngine.Properties.Resources.crystalSemisolidSheet);
        public static Texture2D playerSpriteSheet = LoadTextureFromResource(RaylibGameEngine.Properties.Resources.playerSpriteSheet);
        public static Texture2D coinSpriteSheet = LoadTextureFromResource(RaylibGameEngine.Properties.Resources.coinSheet2);
        public static Texture2D grassBackground = LoadTextureFromResource(RaylibGameEngine.Properties.Resources.colouredBackgroundSheet);
        public static Texture2D particleSheet = LoadTextureFromResource(RaylibGameEngine.Properties.Resources.particleSheet);
        public static Texture2D grassDecor = LoadTextureFromResource(RaylibGameEngine.Properties.Resources.grassDecorSheet);
        #endregion

        public static Texture2D LoadTextureFromResource(byte[] imageData, string fileExtension = "png")
        {
            Image i;
            Memory<byte> d = imageData.AsMemory();
            unsafe { i = Raylib.LoadImageFromMemory(fileExtension, new IntPtr(d.Pin().Pointer), d.Length); }
            Texture2D t = Raylib.LoadTextureFromImage(i);
            Raylib.UnloadImage(i);
            return t;
        }
    }

    public struct TextureAtlas
    {
        public Texture2D atlas;

        public List<Rectangle> TextureRecs;

        public TextureAtlas(Texture2D atlas, List<Rectangle> rect)
        {
            this.atlas = atlas;
            TextureRecs = new List<Rectangle>();
        }
    }

    public struct AtlasTexture2D
    {
        public Texture2D texture;

        public uint width;
        public uint height;

        public uint xOffset;
        public uint yOffset;

        public Rectangle SourceRec
        {
            get
            {
                return new Rectangle(xOffset, yOffset, width, height);
            }
            set
            {
                xOffset = (uint)value.x;
                yOffset = (uint)value.y;
                width = (uint)value.width;
                height = (uint)value.height;
            }
        }

        public AtlasTexture2D(Texture2D texture, int width, int height, int xOffset, int yOffset)
        {
            this.texture = texture;
            this.width = (uint)width;
            this.height = (uint)height;
            this.xOffset = (uint)xOffset;
            this.yOffset = (uint)yOffset;
        }
    }
}