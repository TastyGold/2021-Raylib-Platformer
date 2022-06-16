using Raylib_cs;
using System.Numerics;
using MathExtras;

namespace Engine
{
    public class ParallaxBackground
    {
        //Variables
        public const string filePrefix = "..\\..\\..\\Assets\\Backgrounds\\";

        public SpriteSheet spritesheet;
        public readonly int layers;
        public float[] parallaxValues; //back to front

        //Methods
        public void Draw(CameraController cam)
        {
            for (int i = 0; i < parallaxValues.Length; i++)
            {
                Rectangle destRec = GetLayerScreenRec(i, cam.Target);
                Vectex bounds = cam.GetWorldBounds();
                destRec.x += bounds.min.X;
                destRec.y += bounds.min.Y;

                Raylib.DrawTextureTiled(
                    spritesheet.texture,
                    spritesheet.GetSourceRec(i),
                    Rendering.GetScreenRect(destRec),
                    Vector2.Zero, 0, Screen.pixelScale, Color.WHITE
                    );
                Rendering.CountDrawCall(spritesheet.texture.id);
            }
        }
        public virtual Rectangle GetLayerScreenRec(int layer, Vector2 cameraPosition)
        {
            float x = -cameraPosition.X * (1 - parallaxValues[layer]);
            float sx = spritesheet.spriteSizeX / Screen.pixelsPerUnit;
            float sy = spritesheet.spriteSizeY / Screen.pixelsPerUnit;

            return new Rectangle((x % sx) - sx, 0, sx * 3, sy);
        }

        //Initialisation
        public ParallaxBackground(string texture, float[] values)
        {
            Texture2D tex = Raylib.LoadTexture(filePrefix + texture);
            int width = tex.width;
            int height = tex.height / values.Length;

            spritesheet = new SpriteSheet(new Vector2Int(width, height), tex);
            parallaxValues = values;
            layers = spritesheet.SpritesHigh;
        }

        public ParallaxBackground(Texture2D texture, float[] values)
        {
            Texture2D tex = texture;
            int width = tex.width;
            int height = tex.height / values.Length;

            spritesheet = new SpriteSheet(new Vector2Int(width, height), tex);
            parallaxValues = values;
            layers = spritesheet.SpritesHigh;
        }
    }

    public class ScrollingParallaxBackground : ParallaxBackground
    {
        //Variables
        public float[] scrollingValues;

        //Methods
        public override Rectangle GetLayerScreenRec(int layer, Vector2 cameraPosition)
        {
            float x = -cameraPosition.X * (1 - parallaxValues[layer]);
            float sx = 25;
            float sy = 14.0625f;
            x += Clock.GameTime * 0.016f * scrollingValues[layer];

            return new Rectangle((x % sx) - sx, 0, sx * 3, sy);
        }

        //Initialisation
        public ScrollingParallaxBackground(string texture, float[] values, float[] scrollingValues) : base(texture, values)
        {
            this.scrollingValues = scrollingValues;
        }
        public ScrollingParallaxBackground(Texture2D texture, float[] values, float[] scrollingValues) : base(texture, values)
        {
            this.scrollingValues = scrollingValues;
        }
    }
}