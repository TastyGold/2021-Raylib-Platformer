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
        private void HandleCollision(float deltaTime, out bool colliding, out Vector2 escapeVector)
        {
            escapeVector = Vector2.Zero;
            colliding = false;

            Scene scene = Gameplay.gameplayLevel.ActiveScene;

            //Semisolid collision
            if (velocity.Y <= 0)
            {
                List<Semisolid> overlappingSemisolids = scene.semisolids.GetSemisolidOverlaps(GetHitboxVectexPadded(0.5f));
                foreach (Semisolid s in overlappingSemisolids)
                {
                    if (s.hasSurface && s.x + Semisolid.colliderTrimming < hitbox.X + hitbox.SizeX && s.x + s.width - Semisolid.colliderTrimming > hitbox.X)
                    {
                        float surfaceLeniency = velocity.Y < -1f ? 0 : Semisolid.surfaceDepthLeniency;
                        if (hitbox.Y - (velocity.Y * deltaTime * 2) >= s.y + s.height - surfaceLeniency && hitbox.Y < s.y + s.height)
                        {
                            escapeVector.Y = s.y + s.height - hitbox.Y + float.Epsilon;
                            colliding = true;
                        }
                    }
                }
            }

            //Tile collision
            List<Vector2> collisionTileChecks = GetTileOverlaps(0.25f);
            foreach (Vector2 vector in collisionTileChecks)
            {
                Vector2 v = new Vector2(vector.X, vector.Y);

                if (scene.mainTilemap.GetTileData((int)v.X, (int)v.Y).HasValue && IsOverlapping(v, out float xDist, out float yDist))
                {
                    //Whichever axis the player's hitbox is shallower in will determine the direction of the vector
                    if (Math.Abs(yDist) - (hitbox.Transform.Size.Y * 0.5f) > Math.Abs(xDist) - (hitbox.Transform.Size.X * 0.5f) + deltaTime)
                    {
                        bool escapeFromAbove = yDist <= 0;
                        if (!scene.mainTilemap.GetTileData((int)v.X, (int)v.Y + (escapeFromAbove ? 1 : -1)).HasValue)
                        {
                            escapeVector.Y = yDist + (escapeFromAbove ? minTileDist.Y : -minTileDist.Y);
                            colliding = true;
                        }
                    }
                    else if (Math.Abs(yDist) - (hitbox.Transform.Size.Y * 0.5f) < Math.Abs(xDist) - (hitbox.Transform.Size.X * 0.5f) - deltaTime)
                    {
                        bool escapeFromRight = xDist < 0;
                        if (!scene.mainTilemap.GetTileData((int)v.X + (escapeFromRight ? 1 : -1), (int)v.Y).HasValue)
                        {
                            escapeVector.X = xDist + (escapeFromRight ? minTileDist.X : -minTileDist.X);
                            colliding = true;
                        }
                    }
                }
            }

            Position += escapeVector;
        }

        public override void OnColliding(EntityManagement.Collider2D e)
        {
            if (e is EntityManagement.Spring)
            {
                velocity.Y = 16;
                jumpHeld = false;
            }
        }
    }
}