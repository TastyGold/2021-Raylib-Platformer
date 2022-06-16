using Levels;
using MathExtras;
using System.Numerics;
using Engine;
using System;

namespace Player
{
    public partial class PlayerCharacter : EntityManagement.Collider2D
    {
        private Vector2 floorDetectionSize = new Vector2(0.85f, 0.05f);
        private Vector2 floorDetectionOffset => Vect.Down * (hitbox.Transform.Size.Y / 2 + 0.0625f);
        public bool IsGrounded => DetectFloor();

        private bool groundedByCollision = false;

        private bool DetectFloor()
        {
            if (Gameplay.gameplayLevel.ActiveScene.mainTilemap.OverlapRec(new Vectex(Position + (floorDetectionOffset - (floorDetectionSize / 2)), Position + (floorDetectionOffset + (floorDetectionSize / 2)))))
            {
                return true;
            }
            return false;
        }

        public void DrawFloorDetection(Raylib_cs.Color color)
        {
            Raylib_cs.Raylib.DrawRectangleV(Rendering.WorldVector(Position) + Rendering.WorldVector(floorDetectionOffset - (floorDetectionSize / 2 * Vect.FlipY)), Vect.FlipY * Rendering.WorldVector(floorDetectionSize), color);
            Rendering.CountDrawCallSimple();        
        }
    }
}