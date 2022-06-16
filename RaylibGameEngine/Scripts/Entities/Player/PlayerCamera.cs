using Engine;
using System;
using System.Numerics;
using Raylib_cs;
using Levels;
using MathExtras;

namespace Player
{
    public partial class PlayerCharacter : EntityManagement.Collider2D
    {
        //Config
        public CameraController playerCamera = new CameraController()
        {
            Zoom = 1,
            Target = Vector2.Zero
        };

        private const float cameraHorizontalLerpSpeed = 5f;
        private const float cameraVerticalCatchupSpeed = 4f;
        private const float cameraAscendingCatchupDistance = 2f;
        private const float cameraDescengingCatchupDistance = 1f;
        private Vector2 cameraOffset = Vect.Up * 0.75f;

        //Runtime
        private Vector2 cameraTarget;
        private float cameraHeight = 30;

        //Methods
        public void HandleCameraMovement()
        {
            //cameraTarget.X = MathP.Lerp(cameraTarget.X, position.X, cameraHorizontalLerpSpeed * Clock.DeltaTime);
            cameraTarget.X = Position.X;

            if (cameraHeight > Position.Y + cameraDescengingCatchupDistance)
            {
                cameraHeight = Position.Y + cameraDescengingCatchupDistance;
            }
            else if (cameraHeight < Position.Y - cameraAscendingCatchupDistance)
            {
                cameraHeight = Position.Y - cameraAscendingCatchupDistance;
            }
            else if (groundedByCollision && cameraHeight < Position.Y)
            {
                cameraTarget.Y = MathP.Lerp(cameraTarget.Y, cameraHeight, cameraVerticalCatchupSpeed / 2 * Clock.DeltaTime);
            }

            cameraTarget.Y = MathP.Lerp(cameraTarget.Y, cameraHeight, cameraVerticalCatchupSpeed * Clock.DeltaTime);

            UpdateCameraPosition();
        }
        public void UpdateCameraPosition()
        {
            playerCamera.Target = cameraTarget + cameraOffset;
        }
    }
}