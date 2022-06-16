using System;
using System.Numerics;
using System.Collections.Generic;
using Raylib_cs;
using Levels;
using Engine;
using static Levels.EntityManagement;

namespace Player
{
    public partial class PlayerCharacter : Collider2D
    {
        private Hitbox2D leftWallDetector = new Hitbox2D(0, 0, 3 * Screen.pxl, 2 * Screen.pxl);
        private Hitbox2D rightWallDetector = new Hitbox2D(0, 0, 3 * Screen.pxl, 2 * Screen.pxl);

        private Vector2 leftWallDetectorOffset = new Vector2(-8, -3) * Screen.pxl;
        private Vector2 rightWallDetectorOffset = new Vector2(5, -3) * Screen.pxl;

        private void WallJump(bool right)
        {
            jumpedFromHeight = Position.Y;
            currentJumpRise = 1;
            jumpHeld = true;
            velocity = new Vector2(8f * (right ? -1 : 1), 10.5f);
        }

        private void HandleWallJumping()
        {
            bool slidingLeft = CanWallJump(false);
            bool slidingRight = CanWallJump(true);
            if ((velocity.Y < 0) && (slidingRight || slidingLeft))
            {
                velocity.Y = Math.Max(-8, velocity.Y);
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE))
                {
                    WallJump(slidingLeft ? false : true);
                }
            }
        }

        private void UpdateWallDetectors()
        {
            leftWallDetector.Transform.Position = this.Position + leftWallDetectorOffset;
            rightWallDetector.Transform.Position = this.Position + rightWallDetectorOffset;
        }

        public bool CanWallJump(bool right)
        {
            return AnyTileOverlaps(right ? rightWallDetector : leftWallDetector);
        }

        private void DrawWallDetectors()
        {
            Rectangle r = Rendering.GetScreenRect(leftWallDetector.Transform.Position, leftWallDetector.Transform.Size);
            Rectangle r2 = Rendering.GetScreenRect(rightWallDetector.Transform.Position, rightWallDetector.Transform.Size);
            Raylib.DrawRectangleLinesEx(r, 1, CanWallJump(right: false) ? Color.ORANGE : Color.WHITE);
            Raylib.DrawRectangleLinesEx(r2, 1, CanWallJump(right: true) ? Color.ORANGE : Color.WHITE);
            Rendering.CountDrawCallSimple(2);
        }
    }
}
