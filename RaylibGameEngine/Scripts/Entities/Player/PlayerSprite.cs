using System;
using System.Numerics;
using System.Collections.Generic;
using Engine;
using Raylib_cs;
using Levels;
using MathExtras;

namespace Player
{
    public partial class PlayerCharacter : EntityManagement.Collider2D
    {
        //Configuration
        public static SpriteSheet spriteSheet = new SpriteSheet(new Vector2Int(16, 24), FileManager.assetsDir + "Player\\playerSpriteSheet.png");
        public Vector2 spriteOffset;
        public Animation walkAnim = new Animation(4, 12, new Vector2Int(3, 0));

        public Rectangle GetCurrentSpriteRec()
        {
            Vector2Int spritePos = new Vector2Int();
            float xv = Math.Abs(velocity.X);

            if (!isCrouching)
            {
                if (groundedByCollision)
                {
                    if (xv > 1f )// && xv < 10f)
                    {
                        walkAnim.framesPerSecond = 6 + xv; //walking
                        spritePos.X = walkAnim.GetCurrentFrame().X;
                    }
                    else
                    {
                        spritePos.X = 0;
                    }
                }
                else
                {
                    if (velocity.Y < -6f)
                    {
                        spritePos.X = 2; //falling
                    }
                    else
                    {
                        spritePos.X = 0; //in the air
                    }
                }
            }
            else
            {
                spritePos.X = 1;
            }

            spritePos.Y = facingRight ? 0 : 1;

            return new Rectangle(
                spritePos.X * spriteSheet.spriteSizeX,
                spritePos.Y * spriteSheet.spriteSizeY,
                spriteSheet.spriteSizeX,
                spriteSheet.spriteSizeY);
        }

        public override void Draw()
        {
            if (Gameplay.drawHitboxes)
            {
                playerTrail.Draw();
            }

            Rectangle scr = Rendering.GetScreenRect(Position.X + spriteOffset.X, Position.Y + spriteOffset.Y, 1, 1.5f);

            Raylib.DrawTexturePro(spriteSheet.texture, GetCurrentSpriteRec(), scr, Vector2.Zero, 0, Color.WHITE);
            Rendering.CountDrawCall(spriteSheet.texture.id);

            if (Gameplay.drawHitboxes)
            {
                hitbox.Draw(Color.WHITE);
                DrawWallDetectors();
            }
        }
    }
}