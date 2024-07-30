using System;
using System.Numerics;
using System.Collections.Generic;
using Engine;
using Raylib_cs;
using Levels;
using MathExtras;
using static Levels.EntityManagement;

namespace Player
{
    public static class PlayerData
    {

    }

    public partial class PlayerCharacter : Collider2D
    {
        //Entity info
        public override byte GetEntityID() => 0;
        public override Vector2 GetPositionOffset() => Vector2.Zero;
        private Trail playerTrail = new Trail(120, Color.WHITE);

        //Movement Profiles
        private MovementProfile walk = new MovementProfile()
        {
            groundSpeed = 7f,
            airSpeed = 7f,
            groundAcceleration = 20f,
            groundDeceleration = 30f,
            airAcceleration = 15f,
            airDeceleration = 20f,
            groundHardTurn = 50f,
            airHardTurn = 30f,
        };
        private MovementProfile sprint = new MovementProfile()
        {
            groundSpeed = 9.5f,
            airSpeed = 9.5f,
            groundAcceleration = 5f,
            groundDeceleration = 40f,
            airAcceleration = 1.5f,
            airDeceleration = 22f,
            groundHardTurn = 40f,
            airHardTurn = 30f,
        };
        private MovementProfile crouch = new MovementProfile()
        {
            groundSpeed = 0f,
            airSpeed = 8f,
            groundAcceleration = 7f,
            groundDeceleration = 14f,
            airAcceleration = 6f,
            airDeceleration = 20f,
            groundHardTurn = 20f,
            airHardTurn = 15f,
        };
        private MovementProfile ActiveMovementProfile => isCrouching ? crouch :
            (Math.Abs(velocity.X) >= walk.groundSpeed ? sprint : walk);

        //Moving
        private static bool hardTurning;
        private static float hardTurningSpeed;
        private static bool hardTurningRight;
        private static bool facingRight = true;

        //Crouching
        private static bool isCrouching = false;

        //Jumping
        private const float gravity = 35;
        private const float jumpForce = 10.5f;
        private const float jumpRiseTiles = 1.2f;
        private const float sprintJumpRise = 0.8f;
        private const float minHighJumpSpeed = 3f;
        private const float jumpHeldGravityMod = 0.5f;
        private const float jumpCancelStopper = 0.6f;
        private const float terminalVelocity = 14f;

        private const int graceFrames = Gameplay.targetFPS / 10;
        private int currentGraceFrames = graceFrames;
        private const int bufferFrames = (int)(Gameplay.targetFPS / 7.5f);
        private int currentBufferFrames = bufferFrames;
        private bool jumpHeld = false;

        //Jumprise
        private float currentJumpRise = jumpRiseTiles;
        private float GetJumpRise()
        {
            if (groundedByCollision)
            {
                currentJumpRise = MathP.Lerp(jumpRiseTiles, jumpRiseTiles + sprintJumpRise,
                Math.Min(Math.Max(Math.Abs(velocity.X) - minHighJumpSpeed, 0) / (sprint.groundSpeed - minHighJumpSpeed), 1));
            }
            return currentJumpRise;
        }
        private float jumpedFromHeight;

        //Runtime
        public Vector2 velocity = Vector2.Zero;

        //Methods
        public override void RunBehaviour()
        {
            //Editor behaviour
            if (EntryPoint.startupMode == EntryPoint.LaunchMode.Editor)
            {
                if (Raylib.IsKeyDown(KeyboardKey.KEY_RIGHT))
                    Position += new Vector2(1, 0) * 5 * Clock.DeltaTime;
                if (Raylib.IsKeyDown(KeyboardKey.KEY_UP))
                    Position += new Vector2(0, 1) * 5 * Clock.DeltaTime;
                if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT))
                    Position += new Vector2(-1, 0) * 5 * Clock.DeltaTime;
                if (Raylib.IsKeyDown(KeyboardKey.KEY_DOWN))
                    Position += new Vector2(0, -1) * 5 * Clock.DeltaTime;
                return;
            }

            if (Gameplay.debugEnabled && Raylib.IsMouseButtonPressed(MouseButton.MOUSE_MIDDLE_BUTTON))
                MouseTeleport();

            //Gather input
            Vector2 inputVector = Input.GetWASDInput();

            //Gameplay behaviour
            HandleMovement(inputVector, Gameplay.executedFrames);
            HandleCameraMovement();

            if (velocity.X > 0) facingRight = true;
            if (velocity.X < 0) facingRight = false;

            //Debug
            if (Gameplay.debugEnabled)
            {
                DebugText.WriteLate("Velocity X", Math.Round(velocity.X, 2).ToString());
                DebugText.WriteLate("Velocity Y", Math.Round(velocity.Y, 2).ToString());
                DebugText.WriteLate("H.Turning", hardTurning);
                DebugText.WriteLate("H.T.Speed", Math.Round(hardTurningSpeed, 2));
                DebugText.WriteLate("Jump Rise", Math.Round(GetJumpRise(), 2));
                DebugText.WriteLate("Is rising", !(Position.Y - GetJumpRise() > jumpedFromHeight || !(velocity.Y == jumpForce)));
                DebugText.WriteLate("GraceFrames", currentGraceFrames);
                DebugText.WriteLate("BufferFrames", currentBufferFrames);
            }
        }

        private void HandleMovement(Vector2 input, int iterations)
        {
            float frameTime = Clock.DeltaTime / iterations;
            Vector2 wasdInput = input;

            for (int i = 0; i < iterations; i++)
            {
                bool reached = hardTurning && (
                    (hardTurningRight && velocity.X > hardTurningSpeed) ||
                    (!hardTurningRight && velocity.X < hardTurningSpeed)
                    );

                if (wasdInput.X != 0 && !reached && !wasdInput.X.GetSign().HasSimilarSign(velocity.X.GetSign()))
                {
                    if (!hardTurning)
                    {
                        hardTurning = true;
                        hardTurningRight = wasdInput.X > 0;
                        hardTurningSpeed = -velocity.X / 2;
                        velocity.X *= 0.75f;
                    }
                }
                else
                {
                    hardTurning = false;
                    hardTurningSpeed = 0;
                }

                //Updating velocity based on input
                if (wasdInput.X != 0)
                {
                    float step;
                    if (hardTurning)
                    {
                        step = groundedByCollision ?
                            ActiveMovementProfile.groundHardTurn : ActiveMovementProfile.airHardTurn;
                    }
                    else
                    {
                        step = groundedByCollision ?
                            ActiveMovementProfile.groundAcceleration : ActiveMovementProfile.airAcceleration;
                    }
                    if (Math.Abs(velocity.X) < 0.01f && !isCrouching)
                    {
                        velocity.X = 1 * wasdInput.X;
                    }

                    float profileSpeed = groundedByCollision ?
                        ActiveMovementProfile.groundSpeed : ActiveMovementProfile.airSpeed;

                    velocity.X = MathP.StepTowards(velocity.X, profileSpeed * wasdInput.X, step * frameTime);
                }
                else
                {
                    float profileDeceleration = groundedByCollision ?
                        ActiveMovementProfile.groundDeceleration : ActiveMovementProfile.airDeceleration;

                    velocity.X = MathP.StepTowards(velocity.X, 0, profileDeceleration * frameTime);
                }

                //Ground-specific movement
                if (groundedByCollision)
                {
                    currentGraceFrames = graceFrames;
                    if (velocity.Y < -0.1f)
                        velocity.Y = -0.1f;

                    isCrouching = Raylib.IsKeyDown(KeyboardKey.KEY_S);
                }
                //Airborne-specific movement
                else
                {
                    if (currentGraceFrames > 0)
                        currentGraceFrames--;

                    float g = velocity.Y > 9 && Raylib.IsKeyDown(KeyboardKey.KEY_SPACE) ? gravity * jumpHeldGravityMod : gravity;

                    if (Position.Y - GetJumpRise() > jumpedFromHeight || !(velocity.Y == jumpForce) || !Raylib.IsKeyDown(KeyboardKey.KEY_SPACE))
                    {
                        velocity.Y -= g * frameTime;
                    }

                    if (velocity.Y > 0 && Raylib.IsKeyUp(KeyboardKey.KEY_SPACE) && jumpHeld)
                    {
                        velocity.Y *= jumpCancelStopper;
                        jumpHeld = false;
                    }

                    if (velocity.Y < -terminalVelocity) velocity.Y = -terminalVelocity;

                    if (isCrouching && Raylib.IsKeyUp(KeyboardKey.KEY_S) && velocity.Y < -12f) isCrouching = false;
                }

                //Crouching
                if (isCrouching)
                {
                    hitbox.SizeY = Screen.pxl * 11;
                }
                else
                {
                    hitbox.SizeY = Screen.pxl * 18;
                }

                //Jump
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE))
                {
                    currentBufferFrames = bufferFrames;
                }
                else if (currentBufferFrames > 0)
                    currentBufferFrames--;

                //Jump
                if (currentBufferFrames > 0 && currentGraceFrames > 0)
                {
                    velocity.Y = jumpForce;
                    jumpedFromHeight = Position.Y;
                    currentGraceFrames = 0;
                    currentBufferFrames = 0;
                    jumpHeld = true;
                }

                HandleWallJumping();

                Position += velocity * frameTime;

                HandleCollision(frameTime, out bool colliding, out Vector2 escapeVector);
                if (colliding)
                {
                    //Bang head
                    if (escapeVector.Y < 0 && velocity.Y > 0) velocity.Y = 0;

                    //Stop momentum on sides
                    if ((escapeVector.X < 0 && velocity.X > 0) || (escapeVector.X > 0 && velocity.X < 0)) velocity.X = 0;
                }
                groundedByCollision = colliding && escapeVector.Y > 0;
            }

            //Reset
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_R))
            {
                Position = Gameplay.spawnpoint;
            }

            HandleMoveDetection();
            UpdateWallDetectors();
            playerTrail.AddPoint(Position, MathP.ColorLerp(Color.BLACK, Color.WHITE, MathP.InvLerp(0, 17, velocity.Length())));
        }

        private void MouseTeleport()
        {
            Position = Screen.GetMouseWorldPosition(Gameplay.activeCamera.Camera);
        }
        
        //Initialisation
        public PlayerCharacter()
        {
            hitbox = new Hitbox2D(parent: Transform, localPosition: new Vector2(Screen.pxl * -6, -0.6875f), size: new Vector2(Screen.pxl * 12, Screen.pxl * 18));
            Position = Vector2.Zero;
            spriteSheet.texture = /*Raylib.LoadTexture("..\\..\\..\\Assets\\Player\\girlplayer2.png");/*/ResourceTextures.playerSpriteSheet;
            spriteOffset = new Vector2(-0.5f, -0.6875f);
            OrderInLayer = 100;
            isStationary = false;
            isForceLoaded = true;
        }
    }

    public struct MovementProfile
    {
        public float groundSpeed;
        public float groundAcceleration; //speed units per second
        public float groundDeceleration;
        public float groundHardTurn;

        public float airSpeed;
        public float airAcceleration;
        public float airDeceleration;
        public float airHardTurn;
    }
}