using Engine;
using System;
using System.Numerics;
using Raylib_cs;
using MathExtras;

namespace Levels
{
    public static partial class EntityManagement
    {
        public class Spring : Collider2D
        {
            //EntityInfo
            public override byte GetEntityID() => 4;
            public override Vector2 GetPositionOffset() => new Vector2(0, 0);

            //Configuration
            private static SpriteSheet spriteSheet = new SpriteSheet(new Vector2Int(32, 16), ResourceTextures.crystalSemisolidTileset);
            private readonly Vector2 spriteOffset;

            //Initialisation
            public Spring()
            {
                hitbox = new Hitbox2D(parent: Transform, localPosition: Vector2.Zero, size: new Vector2(2f, 1f));
                spriteOffset = new Vector2(0);
                Position = Vector2.Zero;
                OrderInLayer = 0;
                isStationary = true;
            }

            public override void Draw()
            {
                Rectangle scr = Rendering.GetScreenRect(Position.X + spriteOffset.X, Position.Y + spriteOffset.Y, 2, 1);

                Raylib.DrawTexturePro(spriteSheet.texture, spriteSheet.GetSourceRec(0, 0), scr, Vector2.Zero, 0, Color.WHITE);
                Rendering.CountDrawCall(spriteSheet.texture.id);

                if (Gameplay.drawHitboxes)
                {
                    hitbox.Draw(Color.RED);
                }
            }

            public override void OnColliding(Collider2D e)
            {
                //do nothing
            }

            public override void RunBehaviour()
            {
                //do nothing
            }
        }
    }
}