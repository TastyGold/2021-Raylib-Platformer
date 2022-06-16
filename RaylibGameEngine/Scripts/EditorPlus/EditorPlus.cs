using System;
using System.Collections.Generic;
using System.Numerics;
using System.Diagnostics;
using System.Threading;
using Levels;
using Raylib_cs;
using MathExtras;
using Player;
using PGui;
using static PGui.InputHelper;

namespace Engine
{
    public static partial class EditorPlus
    {
        //Configuration Options
        private const int targetFramerate = 240;
        public static DebugCamera sceneCamera = new DebugCamera();
        //private static Color skyboxColor = new Color(31, 57, 61, 255);
        private static Color skyboxColor = new Color(184, 204, 216, 255);
        private static Color outlineColor = new Color(80, 80, 80, 200);
        private static Color toolColor = new Color(80, 80, 80, 50);
        private static bool drawGrid = true;

        //Runtime Variables
        private static Vector2? mouseDownWorldPosition = null;
        private static Vector2? mouseLastWorldPosition = null;
        private static object heldObject = null;
        private static bool drawingSemisolid = false;
        private static Level editorLevel;
        private static Tilemap activeTilemap;

        //Window layout
        private static Window headerWindow, toolsWindow, editorWindow, propertiesWindow;
        private static DividedWindow w1, w2, w3;
        private static WindowLayout editorLayout;
        private static UIButtonGroup toolButtons;

        //Editor tool bodge
        private static EditTool tempEdit = new EllipseTool();

        //Initialisation method
        public static void InitialiseEditor()
        {
            Console.WriteLine("");
            Console.WriteLine("EDITOR: Initialising window layout");

            headerWindow = new Window(Color.LIGHTGRAY);
            toolsWindow = new Window(Color.LIGHTGRAY);
            propertiesWindow = new Window(Color.LIGHTGRAY);
            editorWindow = new Window(skyboxColor);

            w1 = new DividedWindow(0, 0, Screen.screenWidth, Screen.screenHeight, editorWindow, toolsWindow, Screen.screenHeight - 190, DividedWindow.DividerMode.Vertical);
            w2 = new DividedWindow(0, 0, Screen.screenWidth, Screen.screenHeight, w1, propertiesWindow, Screen.screenWidth - 250, DividedWindow.DividerMode.Horizontal);
            w3 = new DividedWindow(0, 0, Screen.screenWidth, Screen.screenHeight, headerWindow, w2, 40, DividedWindow.DividerMode.Vertical);

            w3.isDividerLocked = true;

            editorLayout = new WindowLayout(w3);
            editorLayout.InitialiseRenderTextures();
            InitialiseToolsWindow();
        }

        public static void InitialiseToolsWindow()
        {
            toolButtons = new UIButtonGroup(new string[3] { "Brush", "Rectangle", "Ellipse" }, toolsWindow, new Rectangle(10, 10, 100, 100));
            toolButtons.buttons["Brush"].OnButtonPressed += () => tempEdit = new BrushTool();
            toolButtons.buttons["Rectangle"].OnButtonPressed += () => tempEdit = new RectangleTool();
            toolButtons.buttons["Ellipse"].OnButtonPressed += () => tempEdit = new EllipseTool();
        }

        //Launch method
        public static void Launch()
        {
            //Initialisation
            Raylib.InitWindow(Screen.screenWidth, Screen.screenHeight, "EditorPlus");
            Raylib.SetTargetFPS(targetFramerate);
            Raylib.SetExitKey(KeyboardKey.KEY_ESCAPE);
            InitialiseEditor();
            sceneCamera.cam.zoom = 0.5f;

            editorLevel = FileManager.ReadLevelFromFile(EntryPoint.levelPath);
            LoadScene(0);

            KeyboardKey? confirmationKey = null;
            Stopwatch confirmationStopwatch = new Stopwatch();

            UILabel alice = new UILabel("Level Editor", 30, 10, new Vector2(10, 10), propertiesWindow, UIHandle.topLeft);
            UIButton steve = new UIButton("Load Scene", 10, 8 , new Vector2(10, 140), propertiesWindow, UIHandle.topLeft);
            steve.OnButtonPressed += Steve_OnButtonPressed;
            steve.OnButtonPressed += () => Console.WriteLine("dnsjkanda");
            UICheckbox bob = new UICheckbox(new Rectangle(10, 190, 30, 30), propertiesWindow, UIHandle.topLeft);

            //Update loop
            while (!Raylib.WindowShouldClose())
            {
                //Diagnostics
                Rendering.ResetDrawCalls();

                //Gather input
                Vector2 mousePosition = sceneCamera.GetMouseWorldPosition(editorWindow, out _);
                editorLayout.RefreshMousePriority();
                editorLayout.mouseHandler.UpdateMouseSprite();
                editorLayout.mouseHandler.RecordMousePosition();
                editorLayout.HandleResizing();

                if (editorWindow.prioritised && editorLayout.mouseHandler.priorityMode == MousePriority.Window)
                    tempEdit.RunToolBehvaiour(mousePosition, editorLevel.ActiveScene); //bodge

                if (propertiesWindow.prioritised) steve.Update();
                if (toolsWindow.prioritised) toolButtons.Update();

                //Editor Shortcuts
                if (Input.CtrlDown())
                {
                    if (Raylib.IsKeyPressed(KeyboardKey.KEY_S))
                    {
                        FileManager.SaveLevelToFile(editorLevel, EntryPoint.levelPath);
                    }
                    #region if (Keypad)
                    if (Raylib.IsKeyPressed(KeyboardKey.KEY_KP_1))
                    {
                        LoadSceneSafe(0);
                    }
                    if (Raylib.IsKeyPressed(KeyboardKey.KEY_KP_2))
                    {
                        LoadSceneSafe(1);
                    }
                    if (Raylib.IsKeyPressed(KeyboardKey.KEY_KP_3))
                    {
                        LoadSceneSafe(3);
                    }
                    #endregion
                    if (Raylib.IsKeyPressed(KeyboardKey.KEY_EQUAL))
                    {
                        if (!confirmationKey.HasValue)
                        {
                            Console.WriteLine("EDITOR: Adding new scene, press again to confirm");
                            confirmationKey = KeyboardKey.KEY_EQUAL;
                            confirmationStopwatch.Restart();
                        }
                        else if (confirmationKey.HasValue && (KeyboardKey)confirmationKey == KeyboardKey.KEY_EQUAL)
                        {
                            editorLevel.AddScene(new Scene());
                            Console.WriteLine("EDITOR: Added new scene");
                            confirmationKey = null;
                        }
                    }
                    if (Raylib.IsKeyPressed(KeyboardKey.KEY_MINUS))
                    {
                        if (!confirmationKey.HasValue)
                        {
                            if (editorLevel.NumberOfScenes > 1)
                            {
                                Console.WriteLine("EDITOR: Deleting current, press again to confirm");
                                confirmationKey = KeyboardKey.KEY_MINUS;
                                confirmationStopwatch.Restart();
                            }
                            else
                            {
                                Console.WriteLine("EDITOR: Cannot delete only scene");
                            }
                        }
                        else if (confirmationKey.HasValue && (KeyboardKey)confirmationKey == KeyboardKey.KEY_MINUS)
                        {
                            editorLevel.DeleteActiveScene(editorLevel);
                            Console.WriteLine("EDITOR: Deleted scene");
                            confirmationKey = null;
                        }
                    }
                }
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_G)) drawGrid = !drawGrid;

                if (confirmationKey != null && confirmationStopwatch.ElapsedMilliseconds > 5000)
                {
                    confirmationStopwatch.Stop();
                    confirmationStopwatch.Reset();
                    confirmationKey = null;
                    Console.WriteLine("EDITOR: Cancelled action");
                }

                //Camera movement
                sceneCamera.HandleMovement(mousePosition);

                //Start drawing
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.RAYWHITE);

                // -- EDITOR WINDOW --
                editorWindow.BeginDrawing();
                Raylib.BeginMode2D(sceneCamera.cam);

                if (Input.CtrlDown() && Raylib.IsKeyPressed(KeyboardKey.KEY_Z))
                {
                    ActionHistory.UndoLastAction(editorLevel.ActiveScene);
                }
                if (Input.CtrlDown() && Raylib.IsKeyPressed(KeyboardKey.KEY_Y))
                {
                    ActionHistory.RedoNextAction(editorLevel.ActiveScene);
                }

                if (drawGrid) activeTilemap.DrawGrid(sceneCamera.cam.zoom);
                editorLevel.DrawActiveScene();
                if (editorWindow.prioritised && editorLayout.mouseHandler.priorityMode == MousePriority.Window) 
                    tempEdit.DrawActionOverlay(sceneCamera.cam); //bodge
                Raylib.EndMode2D();

                Raylib.DrawFPS(10, 10);
                Raylib.DrawText($"Draw calls: {Rendering.DrawCalls}", 10, 30, 20, Color.DARKGREEN);
                DebugText.Clear();
                DebugText.Write("Camera zoom", sceneCamera.cam.zoom);
                //DrawToolText(10, 40);

                editorWindow.EndDrawing();

                // -- CONTROLS WINDOW --
                propertiesWindow.HandleScrolling();
                propertiesWindow.BeginDrawing();
                alice.Draw();
                steve.Draw();
                bob.Draw();
                propertiesWindow.EndDrawing();

                // -- HEADER WINDOW --
                headerWindow.BeginDrawing();
                headerWindow.EndDrawing();

                // -- TOOLS WINDOW --
                toolsWindow.BeginDrawing();
                toolButtons.Draw();
                toolsWindow.EndDrawing();

                //End drawing
                editorLayout.DrawAll();

                //Draw Overlays
                editorLayout.mouseHandler.HighlightMousePriority(MousePriority.Divider);

                Raylib.EndDrawing();

                //Next frame prep
                mouseLastWorldPosition = mousePosition;
            }

            //Termination
            Raylib.CloseWindow();
        }

        private static void Steve_OnButtonPressed()
        {
            Console.WriteLine("steve pressed");
        }

        //Scene control
        private static void LoadSceneSafe(int sceneIndex)
        {
            if (editorLevel.GetActiveSceneIndex == sceneIndex)
            {
                Console.WriteLine($"EDITOR: Scene {sceneIndex} already loaded");
            }
            else if (editorLevel.NumberOfScenes <= sceneIndex)
            {
                Console.WriteLine($"EDITOR: Scene {sceneIndex} does not exist");
            }
            else
            {
                LoadScene(sceneIndex);
            }
        }
        private static void LoadScene(int sceneIndex)
        {
            Console.WriteLine($"EDITOR: Loading scene {sceneIndex}");
            editorLevel.LoadEditorScene(sceneIndex);
            activeTilemap = editorLevel.ActiveScene.mainTilemap;
            editorLevel.ActiveScene.RecalculateSemisolidOrdering();
            editorLevel.ActiveScene.BakeLevelTextures();
        }

        private static Semisolid? GetSemisolidAtMouse(Level level, Vector2 mousePosition)
        {
            for (int i = level.ActiveScene.semisolids.Count; i > 0; i--)
            {
                Semisolid s = level.ActiveScene.SemisolidsDescending[i - 1];
                if (s.Overlaps(mousePosition.ToVector2Int()))
                {
                    mouseDownWorldPosition = mousePosition.Floor();
                    return s;
                }
            }
            return null;
        }

        //Interface for objects that can be dragged in the editor
        public interface IDraggable
        {
            public void DragPosition(Vector2Int vector);
        }
    }
}