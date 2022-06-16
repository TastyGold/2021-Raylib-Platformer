using Engine;
using System.Numerics;
using Raylib_cs;
using MathExtras;

namespace Levels
{
    public static partial class EntityManagement
    {
        public class Coin : Collider2D
        {
            //EntityInfo
            public override byte GetEntityID() => 1;
            public override Vector2 GetPositionOffset() => new Vector2(0.5f);

            //Configuration
            private static SpriteSheet spriteSheet = new SpriteSheet(new Vector2Int(16, 16), ResourceTextures.coinSpriteSheet);
            private readonly Vector2 spriteOffset;
            private readonly Animation coinSpin = new Animation(6, 8, Vector2Int.Zero);

            //Initialisation
            public Coin()
            {
                Vector2 size = new Vector2(10, 14) * Screen.pxl;
                hitbox = new Hitbox2D(Transform, size * -0.5f, size);
                spriteOffset = new Vector2(-0.5f, 0.5f);
                Position = Vector2.Zero;
                OrderInLayer = 0;
                isStationary = true;
            }

            //Methods
            public override void RunBehaviour()
            {
                //do nothing
            }
            public override void OnColliding(Collider2D e)
            {
                if (e is Player.PlayerCharacter)
                {
                    Destroy();
                }
            }
            public override void Draw()
            {
                Vector2 destRecPosition = Rendering.WorldVector(Position.X + spriteOffset.X, Position.Y + spriteOffset.Y).RoundXY();
                Rectangle destRec = new Rectangle(destRecPosition.X, destRecPosition.Y, Screen.scalar, Screen.scalar);
                Raylib.DrawTexturePro(spriteSheet.texture, GetCurrentSpriteRec(), destRec, Vector2.Zero, 0, Color.WHITE);
                Rendering.CountDrawCall(spriteSheet.texture.id);

                if (Gameplay.drawHitboxes)
                {
                    hitbox.Draw(Color.YELLOW);
                }
            }
            public override void DrawInEditor()
            {
                base.DrawInEditor();
            }

            //Functions
            public Rectangle GetCurrentSpriteRec()
            {
                return new Rectangle((coinSpin.GetCurrentFrame().X - (int)Position.X - (int)Position.Y) * 16, 0, 16, 16);
            }
        }
    }
}