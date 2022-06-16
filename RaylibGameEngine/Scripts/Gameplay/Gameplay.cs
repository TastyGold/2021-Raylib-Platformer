using System;
using Engine;
using Levels;
using Raylib_cs;
using System.Collections.Generic;
using System.Numerics;
using Player;
using MathExtras;

namespace Engine
{
    public static class Gameplay
    {
        public static bool debugEnabled = true;
        public static bool drawHitboxes = false;
        public static bool drawCollisionMap = false;

        public const int targetFPS = 60;
        private const float targetFrameTime = 1 / (float)targetFPS;
        private const float maxDroppedFrames = 3;
        public static int executedFrames = 1;

        public static Level gameplayLevel;
        public static CameraController activeCamera;
        
        private static readonly ScrollingParallaxBackground sceneBackground = new ScrollingParallaxBackground(
            ResourceTextures.grassBackground,
            new float[4] { 1f, 0.96f, 0.92f, 0.84f },
            new float[4] { 0f, -0.016f, -0.025f, 0f }
            );

        public static Vector2 spawnpoint = Vector2.One * 10;

        public static void Launch()
        {
            Raylib.InitWindow(Screen.screenWidth, Screen.screenHeight, "Player");
            Raylib.SetTargetFPS(targetFPS);

            gameplayLevel = FileManager.ReadLevelFromFile(EntryPoint.levelPath);
            //gameplayLevel = FileManager.ReadLevelFromResource(RaylibGameEngine.Properties.Resources.SampleLevel);
            LoadScene(0);

            EntityManagement.ParticleSystem ps = new EntityManagement.ParticleSystem() { Position = new Vector2(6, 10) };
            for (int i = 0; i < 5; i++)
            {
                ps.particles.Add(new EntityManagement.Particles.Moth(new Vectex(ps.Position - Vector2.One, ps.Position + new Vector2(1, 1)), gameplayLevel.ActiveScene.playerReference));
            }
            gameplayLevel.ActiveScene.AddEntity(ps, ps.Position);
            gameplayLevel.ActiveScene.SortOrderInLayer();

            Clock.Start();
            while (!Raylib.WindowShouldClose())
            {
                //Diagnostics + Debug
                Rendering.ResetDrawCalls();
                DebugText.Clear();
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_F1)) debugEnabled = !debugEnabled;
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_F2)) drawHitboxes = !drawHitboxes;
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_F3)) activeCamera.isWideView = !activeCamera.isWideView;
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_F4)) drawCollisionMap = !drawCollisionMap;

                //Compensate fram drops
                if (Clock.DeltaTime >= targetFrameTime * maxDroppedFrames)
                {
                    executedFrames = (int)Math.Round(Clock.DeltaTime / targetFrameTime);
                    Console.WriteLine($"Frame drop: {(int)(Clock.DeltaTime * 1000)}ms ({Math.Round(Clock.DeltaTime / targetFrameTime) - 1})");
                }
                else
                {
                    executedFrames = 1;
                }

                //Handle game logic
                gameplayLevel.ActiveScene.Update();
                gameplayLevel.ActiveScene.collisionMap.UpdateEntityCells();
                Vectex cameraBounds = activeCamera.GetWorldBounds();
                gameplayLevel.ActiveScene.collisionMap.SetLoadedRegion(UniformGrid.GetGridCell(cameraBounds.min), UniformGrid.GetGridCell(cameraBounds.max));

                //Begin drawing
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.RAYWHITE);

                //Render scene
                Raylib.BeginMode2D(activeCamera.Camera);
                sceneBackground.Draw(activeCamera);
                if (drawCollisionMap) gameplayLevel.ActiveScene.collisionMap.DrawGrid((int)Math.Ceiling(2/activeCamera.Zoom), true);
                gameplayLevel.DrawActiveScene();
                if (activeCamera.isWideView) activeCamera.DrawCameraBounds();

                Raylib.EndMode2D();

                //Render overlays
                if (debugEnabled)
                {
                    DebugText.DrawBackground();
                    DebugText.WriteTitle("DEBUG MODE (F1)");
                    DebugText.WriteFPS();
                    DebugText.Write("Draw calls", Rendering.DrawCalls);
                    DebugText.Write("Draw Batches", Rendering.Batches);
                    DebugText.Write("Delta Time", Clock.DeltaTime);
                    DebugText.Write("Pos", gameplayLevel.ActiveScene.playerReference.Position.ToString(1));
                    DebugText.WriteTickets();
                }

                //Stop drawing
                Raylib.EndDrawing();

                //Editor Shortcuts
                if (Input.CtrlDown())
                {
                    if (Raylib.IsKeyPressed(KeyboardKey.KEY_KP_1))
                    {
                        LoadSceneSafe(0);
                    }
                    if (Raylib.IsKeyPressed(KeyboardKey.KEY_KP_2))
                    {
                        LoadSceneSafe(1);
                    }
                }

                //Clock
                Clock.Count();
                Clock.AdvanceDeltaTime();
            }

            Raylib.CloseWindow();
        }

        //Scene control
        private static void LoadSceneSafe(int sceneIndex)
        {
            if (gameplayLevel.GetActiveSceneIndex == sceneIndex)
            {
                Console.WriteLine($"GAMEPLAY: Scene {sceneIndex} already loaded");
            }
            else if (gameplayLevel.NumberOfScenes <= sceneIndex)
            {
                Console.WriteLine($"GAMEPLAY: Scene {sceneIndex} does not exist");
            }
            else
            {
                LoadScene(sceneIndex);
            }
        }
        private static void LoadScene(int sceneIndex)
        {
            Console.WriteLine($"GAMEPLAY: Loading scene {sceneIndex}");
            gameplayLevel.LoadEditorScene(sceneIndex);
            gameplayLevel.ActiveScene.BakeLevelTextures();
            SpawnPlayer();
            activeCamera = gameplayLevel.ActiveScene.playerReference.playerCamera;
            gameplayLevel.ActiveScene.SortOrderInLayer();
        }

        //Scene transition for player
        private static void SpawnPlayer()
        {
            if (!gameplayLevel.ActiveScene.hasPlayer)
            {
                gameplayLevel.ActiveScene.AddPlayer(new Vector2(10));
            }
        }
    }
}