using System;
using MathExtras;
using Raylib_cs;

namespace Engine
{
    public struct SpriteSheet
    {
        public Texture2D texture;

        public ushort spriteSizeX;
        public ushort spriteSizeY;

        public ushort SpritesWide => (ushort)(texture.width / spriteSizeX);
        public ushort SpritesHigh => (ushort)(texture.height / spriteSizeY);

        public Rectangle GetSourceRec(int index)
        {
            int xPos = index % SpritesWide;
            int yPos = (index - xPos) / SpritesWide;

            return new Rectangle(xPos * spriteSizeX, yPos * spriteSizeY, spriteSizeX, spriteSizeY);
        }
        public Rectangle GetSourceRec(int x, int y)
        {
            if (x > SpritesWide || y > SpritesHigh)
            {
                throw new ArgumentOutOfRangeException();
            }
            return new Rectangle(x * spriteSizeX, y * spriteSizeY, spriteSizeX, spriteSizeY);
        }

        //Initialisation
        public SpriteSheet(Vector2Int spriteRes, string filePath)
        {
            texture = Raylib.LoadTexture(filePath);
            spriteSizeX = (ushort)spriteRes.X;
            spriteSizeY = (ushort)spriteRes.Y;
        }
        public SpriteSheet(int spriteRes, string filePath)
        {
            this = new SpriteSheet(new Vector2Int(spriteRes, spriteRes), filePath);
        }
        public SpriteSheet(Vector2Int spriteRes, Texture2D texture)
        {
            this.texture = texture;
            spriteSizeX = (ushort)spriteRes.X;
            spriteSizeY = (ushort)spriteRes.Y;
        }
        public SpriteSheet(int spriteRes, Texture2D texture)
        {
            this = new SpriteSheet(new Vector2Int(spriteRes, spriteRes), texture);
        }
    }

    public struct SubSpriteSheet
    {
        //Data
        public Texture2D texture;

        public ushort spriteResX;
        public ushort spriteResY;

        public ushort sheetLength;
        public ushort sheetHeight;

        public ushort pixelOffsetX;
        public ushort pixelOffsetY;

        //Function
        public Rectangle GetSourceRec(int index)
        {
            int spriteX = index % sheetLength;
            int spriteY = sheetHeight == 1 ? 0 : (index - spriteX) / sheetLength;

            return new Rectangle(pixelOffsetX + (spriteResX * spriteX), pixelOffsetY + (spriteResY * spriteY), spriteResX, spriteResY);
        }

        //Constructor
        public SubSpriteSheet(Texture2D texture, int spriteResX, int spriteResY, int sheetLength, int sheetHeight, int offsetX, int offsetY)
        {
            this.texture = texture;
            this.spriteResX = (ushort)spriteResX;
            this.spriteResY = (ushort)spriteResY;
            this.sheetLength = (ushort)sheetLength;
            this.sheetHeight = (ushort)sheetHeight;
            this.pixelOffsetX = (ushort)offsetX;
            this.pixelOffsetY = (ushort)offsetY;
        }
    }

    public class Animation
    {
        //Variables
        public readonly int frames;
        public float framesPerSecond;
        public Vector2Int offset;
        public ScrollType scrollType = ScrollType.Horizontal;
        public bool looping = true;
        public Clock.Timestamp lastFrameTime;
        public float FrameDuration => 1 / framesPerSecond;
        public long FrameDurationMs => (long)(1000 / framesPerSecond);

        private int currentFrameIndex = 0;
        public Vector2Int GetCurrentFrame()
        {
            while (Clock.TimeSinceMs(lastFrameTime) > FrameDurationMs)
            {
                lastFrameTime.time += FrameDurationMs;
                currentFrameIndex++;
                if (currentFrameIndex >= frames) currentFrameIndex = 0;
            }

            return offset + (scrollType == ScrollType.Horizontal ? new Vector2Int(currentFrameIndex, 0) : new Vector2Int(0, currentFrameIndex));
        }
        public void AdvanceFrames(int n)
        {
            currentFrameIndex += n;
            currentFrameIndex %= frames;
        }

        //Initialisation
        public Animation(int frames, int framesPerSecond, Vector2Int offset, ScrollType scrollType = ScrollType.Horizontal)
        {
            this.frames = frames;
            this.framesPerSecond = framesPerSecond;
            this.offset = offset;
            this.scrollType = scrollType;
            lastFrameTime = Clock.Now;
        }

        //Scroll types
        public enum ScrollType
        {
            Horizontal,
            Vertical
        }
    }
}