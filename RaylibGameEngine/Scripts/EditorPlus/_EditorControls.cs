using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using Levels;
using Raylib_cs;
using MathExtras;
using Player;

namespace Engine
{
/*
    public static partial class EditorPlus
    {
        private static Tool currentTool = Tool.Brush;
        private static int brushSize = 1;
        private static bool autoTile = true;
        private static int toolMod = 1;

        private static EditAction currentAction;

        private static void RunToolBehaviour(Vector2 mousePosition, Tilemap activeTilemap, Level editorLevel)
        {
            //Handle editor functionality
            if (currentTool == Tool.Brush)
            {
                //Establish and complete action
                if (editorWindow.prioritised && (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON) || Raylib.IsMouseButtonPressed(MouseButton.MOUSE_RIGHT_BUTTON)))
                {
                    currentAction = new SetTilesAction();
                }
                if (!editorWindow.prioritised || Raylib.IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON) || Raylib.IsMouseButtonReleased(MouseButton.MOUSE_RIGHT_BUTTON))
                {
                    ActionHistory.AddAction(currentAction);
                }

                if (editorWindow.prioritised && (Raylib.IsMouseButtonDown(MouseButton.MOUSE_LEFT_BUTTON) ^ Raylib.IsMouseButtonDown(MouseButton.MOUSE_RIGHT_BUTTON)))
                {
                    Vector2 pos = mousePosition.Floor();
                    SetTilesAction action = currentAction as SetTilesAction;
                    for (float x = pos.X - 1; x <= pos.X + 1; x++)
                    {
                        for (float y = pos.Y - 1; y <= pos.Y + 1; y++)
                        {
                            action.ModifyTile(new Vector2(x, y), activeTilemap.GetTileData((int)x, (int)y), null);
                        }
                    }

                    activeTilemap.SetTile((int)pos.X, (int)pos.Y, Raylib.IsMouseButtonDown(MouseButton.MOUSE_LEFT_BUTTON) ? (byte?)0 : null);
                    if (autoTile)
                    {
                        activeTilemap.FormatTilesRec(pos - Vector2.One, pos + Vector2.One);
                        for (float x = pos.X - 1; x <= pos.X + 1; x++)
                        {
                            for (float y = pos.Y - 1; y <= pos.Y + 1; y++)
                            {
                                action.ModifyTile(new Vector2(x, y), null, activeTilemap.GetTileData((int)x, (int)y));
                            }
                        }
                    }
                }
            }
            else if (currentTool == Tool.Area)
            {
                if (editorWindow.prioritised && mouseDownWorldPosition == null && (Input.MouseButtonPressed(0) || Input.MouseButtonPressed(1)))
                {
                    mouseDownWorldPosition = mousePosition.Floor();
                }
                if (mouseDownWorldPosition.HasValue && (Input.MouseButtonReleased(0) || Input.MouseButtonReleased(1)))
                {
                    Vectex v = new Vectex((Vector2)mouseDownWorldPosition, mousePosition.Floor());

                    activeTilemap.SetTilesRec(v.min, v.max, Input.MouseButtonReleased(0) ? (byte?)13 : null);
                    if (autoTile) activeTilemap.FormatTilesRec(v.min - Vector2.One, v.max + Vector2.One);
                }
                if (mouseDownWorldPosition.HasValue && (Input.MouseButtonDown(0) || Input.MouseButtonDown(1)))
                {
                    Vectex v = new Vectex((Vector2)mouseDownWorldPosition, mousePosition.Floor()).SwapY();

                    Raylib.DrawRectangleV(Rendering.WorldVector(v.min + Vect.Up),
                        Rendering.WorldVector(v.max - v.min + Vect.FlipY),
                        toolColor);
                    Rendering.CountDrawCallSimple();
                }
                else if (Input.MouseButtonUp(0) && Input.MouseButtonUp(1))
                {
                    mouseDownWorldPosition = null;
                }
            }
            else if (currentTool == Tool.Semisolid)
            {
                if (editorWindow.prioritised && Input.MouseButtonPressed(0))
                {
                    mouseDownWorldPosition = mousePosition.Floor();
                    Semisolid? clickedObject = GetSemisolidAtMouse(editorLevel, mousePosition);
                    if (clickedObject != null)
                    {
                        heldObject = clickedObject;
                    }
                    else
                    {
                        drawingSemisolid = true;
                    }
                }
                if (editorWindow.prioritised && ((Input.MouseButtonDown(1) && ((Vector2)mouseLastWorldPosition).Floor() != mousePosition.Floor())
                    || Input.MouseButtonPressed(1)))
                {
                    Semisolid? objectToErase = GetSemisolidAtMouse(editorLevel, mousePosition);
                    if (objectToErase != null)
                    {
                        editorLevel.ActiveScene.semisolids.Remove((Semisolid)objectToErase);
                    }
                    editorLevel.ActiveScene.RecalculateSemisolidOrdering();
                }
                if (drawingSemisolid)
                {
                    Vectex v = new Vectex((Vector2)mouseDownWorldPosition, mousePosition.Floor()).SwapY();

                    Raylib.DrawRectangleV(Rendering.WorldVector(v.min + Vect.Up),
                        Rendering.WorldVector(v.max - v.min + Vect.FlipY),
                        toolColor);
                    Rendering.CountDrawCallSimple();
                }
                if (Input.MouseButtonReleased(0))
                {
                    if (heldObject != null && heldObject is Semisolid s)
                    {
                        editorLevel.ActiveScene.semisolids.Remove(s);
                        s.DragPosition((Vector2Int)(mousePosition.Floor() - mouseDownWorldPosition.GetValueOrDefault().Floor()));
                        editorLevel.ActiveScene.semisolids.Add(s);

                        editorLevel.ActiveScene.RecalculateSemisolidOrdering();
                        heldObject = null;
                        mouseDownWorldPosition = null;
                    }
                    else if (drawingSemisolid)
                    {
                        Semisolid newSemisolid = new Semisolid((Vector2)mouseDownWorldPosition, mousePosition.Floor(), true, 0);

                        editorLevel.ActiveScene.semisolids.Add(newSemisolid);

                        editorLevel.ActiveScene.RecalculateSemisolidOrdering();

                        drawingSemisolid = false;
                    }
                }
            }
            else if (currentTool == Tool.Entity)
            {
                if (editorWindow.prioritised && Raylib.IsMouseButtonDown(MouseButton.MOUSE_LEFT_BUTTON))
                {
                    if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON) || 
                        !mousePosition.ToVector2Int().Equals(mouseLastWorldPosition.Value.ToVector2Int()))
                    {
                        if(!editorLevel.ActiveScene.IsOccupied(mousePosition.ToVector2Int(), toolMod))
                        {
                            editorLevel.ActiveScene.AddEntity((EntityManagement.Entity)Activator.CreateInstance(EntityManagement.EntityIDs[toolMod]), mousePosition.Floor() + new Vector2(0.5f));
                        }
                    }
                }
                if (editorWindow.prioritised && Raylib.IsMouseButtonDown(MouseButton.MOUSE_RIGHT_BUTTON))
                {
                    if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_RIGHT_BUTTON) ||
                        !mousePosition.ToVector2Int().Equals(mouseLastWorldPosition.Value.ToVector2Int()))
                    {
                        editorLevel.ActiveScene.RemoveEntityAt(mousePosition.ToVector2Int());
                    }
                }
            }
            else if (currentTool == Tool.Format)
            {
                if (mouseDownWorldPosition == null && editorWindow.prioritised && (Input.MouseButtonPressed(0) || Input.MouseButtonPressed(1)))
                {
                    mouseDownWorldPosition = mousePosition.Floor();
                }
                if (mouseDownWorldPosition.HasValue && (Input.MouseButtonReleased(0) || Input.MouseButtonReleased(1)))
                {
                    Vectex v = new Vectex((Vector2)mouseDownWorldPosition, mousePosition.Floor());

                    activeTilemap.FormatTilesRec(v.min, v.max);
                }
                if (mouseDownWorldPosition.HasValue && (Input.MouseButtonDown(0) || Input.MouseButtonDown(1)))
                {
                    Vectex v = new Vectex((Vector2)mouseDownWorldPosition, mousePosition.Floor()).SwapY();

                    Raylib.DrawRectangleV(Rendering.WorldVector(v.min + Vect.Up),
                        Rendering.WorldVector(v.max - v.min + Vect.FlipY),
                        toolColor);
                    Rendering.CountDrawCallSimple();
                }
                else if (Input.MouseButtonUp(0) && Input.MouseButtonUp(1))
                {
                    mouseDownWorldPosition = null;
                }
            }
        }

    

        //Enum for tools that can be used in the editor
        private enum Tool
        {
            Empty = 0,
            Brush = 1,
            Area = 2,
            Semisolid = 3,
            Entity = 4,
            Format = 5,
        }

        //Tool functions
        private static void DrawToolText(int x, int y)
        {
            Raylib.DrawText("Tool:", x, y, 20, Color.DARKGRAY);
            Raylib.DrawText(currentTool.ToToolText(), x, y + 25, 20, Color.DARKGRAY);
        }
        private static void HandleToolCycling()
        {
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_TAB))
            {
                if (!Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT))
                {
                    currentTool++;
                    if (currentTool > lastTool)
                    {
                        currentTool = firstTool;
                    }
                }
                else
                {
                    currentTool--;
                    if (currentTool < firstTool)
                    {
                        currentTool = lastTool;
                    }
                }
            }
        }
        private static string ToToolText(this Tool tool)
        {
            return tool switch
            {
                Tool.Empty => "Empty",
                Tool.Brush => "Brush",
                Tool.Area => "Area",
                Tool.Semisolid => "Semisolid",
                Tool.Entity => $"Entity ({toolMod})",
                Tool.Format => "Format",
                _ => "Unknown",
            };
        }
        private static readonly Tool firstTool = Tool.Brush;
        private static readonly Tool lastTool = Tool.Format;
    }*/
}