using System;
using System.Numerics;
using Player;
using Raylib_cs;
using Engine;
using MathExtras;

namespace Levels
{
    public static partial class EntityManagement
    {
        public class TriggerColldier : Collider2D
        {
            //Data
            protected PlayerCharacter playerRef;
            public Vector2 triggerSize;

            public override byte GetEntityID() => 2;
            public override Vector2 GetPositionOffset() => new Vector2(0.5f);
            public override void RunBehaviour()
            {

            }

            public override void OnColliding(Collider2D e)
            {
                if (e is PlayerCharacter player)
                {
                    playerRef = player;
                    player.velocity.Y = 10;
                }
            }

            public override void Draw()
            {
                if (Gameplay.drawHitboxes) hitbox.Draw(Color.ORANGE);
            }
            public override void DrawInEditor()
            {
                Raylib.DrawRectangleV((Position + (triggerSize.Y * Vect.Up)) * Screen.scalar * Vect.FlipY, triggerSize * Screen.scalar, new Color(255, 124, 31, 100));
            }

            public TriggerColldier() { }

            public TriggerColldier(Vector2 size)
            {
                triggerSize = size;
                hitbox = new Hitbox2D(Transform, Position, size);
            }
        }
    }
}