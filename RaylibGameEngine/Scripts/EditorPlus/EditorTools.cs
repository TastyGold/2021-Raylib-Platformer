using System;
using System.Numerics;
using System.Collections.Generic;
using Raylib_cs;
using Levels;
using MathExtras;
using static PGui.InputHelper;

namespace Engine
{
    public static partial class EditorPlus
    {
        public enum EditorTool
        {
            //Tilemap
            None,
            Brush,
            Rectangle,
            Ellipse,

            //Objects
            Entity,
            Decor,
            Trigger,

            //Selection
            MouseSelect,
            RectangleSelect,
        }
    }

    public abstract class EditTool
    {
        //Data
        protected bool isActive = false;
        protected State state = State.None;
        protected EditAction currentAction;

        protected Vector2 mousePos;
        protected Vector2 sceneBounds;

        //Public methods
        public virtual void RunToolBehvaiour(Vector2 mouseWorldPosition, Scene activeScene)
        {
            mousePos = mouseWorldPosition;
            sceneBounds = new Vector2(activeScene.mainTilemap.TilemapWidth, activeScene.mainTilemap.TilemapHeight);
            if (Clicked_MB) BeginAction(activeScene);
            if (Held_MB && isActive) ProcessAction(activeScene);
            if (Released_MB) CompleteAction(activeScene);
        }
        public abstract void DrawActionOverlay(Camera2D cam);

        //Tool methods
        protected abstract void BeginAction(Scene activeScene);
        protected abstract void ProcessAction(Scene activeScene);
        protected abstract void CompleteAction(Scene activeScene);
        protected abstract void CancelAction();
        protected void HighlightMouse()
        {
            Vector2 tile = mousePos.Floor();
            Rectangle r = new Rectangle(tile.X * Screen.scalar, (-1 - tile.Y) * Screen.scalar, Screen.scalar, Screen.scalar);
            Raylib.DrawRectangleRec(r, CurrentColor1);
            Raylib.DrawRectangleLinesEx(r, Screen.pixelScale, CurrentColor2);
        }

        public enum State
        {
            None,
            Paint,
            Erase
        }

        //Colors
        public Color CurrentColor1 => state switch
        {
            State.Paint => paintColor1,
            State.Erase => eraseColor1,
            _ => waitColor1,
        }; 
        public Color CurrentColor2 => state switch
        {
            State.Paint => paintColor2,
            State.Erase => eraseColor2,
            _ => waitColor2,
        };
        public readonly Color paintColor1 = new Color(25, 152, 255, 75);
        public readonly Color paintColor2 = new Color(25, 152, 255, 200);
        public readonly Color waitColor1 = new Color(0, 0, 0, 0);
        public readonly Color waitColor2 = new Color(100, 100, 100, 100);
        public readonly Color eraseColor1 = new Color(255, 100, 50, 75);
        public readonly Color eraseColor2 = new Color(255, 100, 50, 200);
    }

    public class BrushTool : EditTool
    {
        //Data
        protected List<Vector2> tilesDirectlyChanged;
        private List<Vector2> tilesAffected;

        //Overrides
        protected override void BeginAction(Scene activeScene)
        {
            currentAction = new SetTilesAction();
            tilesDirectlyChanged = new List<Vector2>();
            tilesAffected = new List<Vector2>();
            isActive = true;
            state = Clicked_LMB ? State.Paint : State.Erase;
        }

        protected override void ProcessAction(Scene activeScene)
        {
            Vector2 pos = mousePos.Floor();

            if (!(pos.X < 0 || pos.X >= sceneBounds.X || pos.Y < 0 || pos.Y >= sceneBounds.Y))
            {
                if (!tilesDirectlyChanged.Contains(pos)) tilesDirectlyChanged.Add(pos);
            }

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    Vector2 v = new Vector2(pos.X + x, pos.Y + y);

                    if (!(v.X < 0 || v.X >= sceneBounds.X || v.Y < 0 || v.Y >= sceneBounds.Y))
                    {
                        if (!tilesAffected.Contains(v)) tilesAffected.Add(v);
                    }
                }
            }
        }

        protected override void CompleteAction(Scene activeScene)
        {
            foreach (Vector2 tile in tilesAffected)
            {
                //Record existing tile states
                (currentAction as SetTilesAction).ModifyTile(tile, activeScene.mainTilemap.GetTileData(tile), null);
            }
            foreach (Vector2 tile in tilesDirectlyChanged)
            {
                //Update tile states
                activeScene.mainTilemap.SetTile((int)tile.X, (int)tile.Y, state == State.Paint ? (byte?)0 : null);
            }
            foreach (Vector2 tile in tilesAffected)
            {
                //Record tile changes
                activeScene.mainTilemap.FormatTile(tile);
                (currentAction as SetTilesAction).ModifyTile(tile, null, activeScene.mainTilemap.GetTileData(tile));
            }

            CancelAction();

            ActionHistory.AddAction(currentAction);
        }

        protected override void CancelAction()
        {
            isActive = false;
            state = State.None;
            tilesDirectlyChanged.Clear();
            tilesAffected.Clear();
        }

        public override void DrawActionOverlay(Camera2D cam)
        {
            if (isActive)
            {
                foreach (Vector2 tile in tilesDirectlyChanged)
                {
                    Rectangle r = new Rectangle(tile.X * Screen.scalar, (-1 - tile.Y) * Screen.scalar, Screen.scalar, Screen.scalar);
                    Raylib.DrawRectangleRec(r, CurrentColor1);
                    if (!(tilesDirectlyChanged.Count > 1000))
                        DrawCrudeAutoOutline(tile);
                    Rendering.CountDrawCallSimple();
                }
            }
            else HighlightMouse();
        }

        //Methods
        protected void DrawCrudeAutoOutline(Vector2 tile)
        {
            if (!tilesDirectlyChanged.Contains(tile + Vect.Up)) Raylib.DrawLineEx((tile + Vect.Up) * Screen.scalar * Vect.FlipY, (tile + Vector2.One) * Screen.scalar * Vect.FlipY, Screen.pixelScale, CurrentColor2);
            if (!tilesDirectlyChanged.Contains(tile + Vect.Left)) Raylib.DrawLineEx(tile * Screen.scalar * Vect.FlipY, (tile + Vect.Up) * Screen.scalar * Vect.FlipY, Screen.pixelScale, CurrentColor2);
            if (!tilesDirectlyChanged.Contains(tile + Vect.Right)) Raylib.DrawLineEx((tile + Vect.Right) * Screen.scalar * Vect.FlipY, (tile + Vector2.One) * Screen.scalar * Vect.FlipY, Screen.pixelScale, CurrentColor2);
            if (!tilesDirectlyChanged.Contains(tile + Vect.Down)) Raylib.DrawLineEx(tile * Screen.scalar * Vect.FlipY, (tile + Vect.Right) * Screen.scalar * Vect.FlipY, Screen.pixelScale, CurrentColor2);
        }
    }

    public class RectangleTool : EditTool
    {
        //Data
        protected Vector2 mouseDownPosition;
        protected Vectex region;

        //Overrides
        protected override void BeginAction(Scene activeScene)
        {
            currentAction = new SetTilesAction();
            isActive = true;
            state = Clicked_LMB ? State.Paint : State.Erase;
            mouseDownPosition = mousePos;
        }

        protected override void ProcessAction(Scene activeScene)
        {
            Vector2 end;
            if (Held_LSHIFT)
            {
                Vector2 regionSize = mousePos.Floor() - mouseDownPosition.Floor();
                int radius = (int)Math.Max(Math.Abs(regionSize.X), Math.Abs(regionSize.Y));
                Vector2 size = new Vector2(regionSize.X > 0 ? 1 : -1, regionSize.Y > 0 ? 1 : -1) * radius;
                end = mouseDownPosition.Floor() + size;
            }
            else end = mousePos.Floor();
            if (Held_LALT)
            {
                Vector2 distance = mouseDownPosition.Floor() - end;
                region = new Vectex(mouseDownPosition.Floor() - distance, mouseDownPosition.Floor() + distance);
            }
            else
            {
                region = new Vectex(mouseDownPosition.Floor(), end);
            }
        }

        protected override void CompleteAction(Scene activeScene)
        {
            SetTilesAction action = currentAction as SetTilesAction;

            for (int x = (int)region.min.X - 1; x <= (int)region.max.X + 1; x++)
            {
                for (int y = (int)region.min.Y - 1; y <= (int)region.max.Y + 1; y++)
                {
                    //Record existing tile states
                    action.ModifyTile(new Vector2(x, y), activeScene.mainTilemap.GetTileData(x, y), null);
                }
            }

            //Update tile states
            activeScene.mainTilemap.SetTilesRec(region.min, region.max, state == State.Paint ? (byte?)9 : null);

            for (int x = (int)region.min.X - 1; x <= (int)region.max.X + 1; x++)
            {
                for (int y = (int)region.min.Y - 1; y <= (int)region.max.Y + 1; y++)
                {
                    //Record tile changes
                    if (x <= region.min.X || x >= region.max.X || y <= region.min.Y || y >= region.max.Y)
                    {
                        activeScene.mainTilemap.FormatTile(x, y);
                    }
                    action.ModifyTile(new Vector2(x, y), null, activeScene.mainTilemap.GetTileData(x, y));
                }
            }

            CancelAction();
            ActionHistory.AddAction(currentAction);
        }

        protected override void CancelAction()
        {
            isActive = false;
            state = State.None;
        }

        public override void DrawActionOverlay(Camera2D cam)
        {
            if (isActive)
            {
                Rectangle r = new Rectangle(region.min.X * Screen.scalar, (-region.min.Y - region.Size.Y - 1) * Screen.scalar, (region.Size.X + 1) * Screen.scalar, (region.Size.Y + 1) * Screen.scalar);
                Raylib.DrawRectangleRec(r, CurrentColor1);
                Raylib.DrawRectangleLinesEx(r, Screen.pixelScale, CurrentColor2);
            }
            else HighlightMouse();
        }
    }

    public class EllipseTool : BrushTool
    {
        //Data
        private Vector2 mouseDownPosition;
        private Vectex region;
        private Ellipse ellipse;

        //Overrides
        protected override void BeginAction(Scene activeScene)
        {
            base.BeginAction(activeScene);
            mouseDownPosition = mousePos;
        }

        protected override void ProcessAction(Scene activeScene)
        {
            Vector2 end;
            if (Held_LSHIFT)
            {
                Vector2 regionSize = mousePos.Floor() - mouseDownPosition.Floor();
                int radius = (int)regionSize.Length();
                Vector2 size = new Vector2(regionSize.X > 0 ? 1 : -1, regionSize.Y > 0 ? 1 : -1) * radius;
                end = mouseDownPosition.Floor() + size;
            }
            else end = mousePos.Floor();
            if (Held_LALT)
            {
                Vector2 distance = mouseDownPosition.Floor() - end;
                region = new Vectex(mouseDownPosition.Floor() - distance, mouseDownPosition.Floor() + distance);
            }
            else
            {
                region = new Vectex(mouseDownPosition.Floor(), end);
            }
            tilesDirectlyChanged = GetEnclosedTiles();
        }

        protected override void CompleteAction(Scene activeScene)
        {
            SetTilesAction action = currentAction as SetTilesAction;

            Dictionary<Vector2, bool> tilesAffected = new Dictionary<Vector2, bool>();

            //Record existing tile states
            foreach (Vector2 tile in tilesDirectlyChanged)
            {
                for (int x = (int)tile.X - 1; x <= (int)tile.X + 1; x++)
                {
                    for (int y = (int)tile.Y - 1; y <= (int)tile.Y + 1; y++)
                    {
                        tilesAffected[new Vector2(x, y)] = true;
                        action.ModifyTile(new Vector2(x, y), activeScene.mainTilemap.GetTileData(x, y), null); 
                    }
                }
            }
            //Apply tilemap changes
            foreach (Vector2 tile in tilesDirectlyChanged)
            {
                activeScene.mainTilemap.SetTile(tile, state == State.Paint ? (byte?)0 : null);
            }
            //Record new tile states
            foreach (KeyValuePair<Vector2, bool> tile in tilesAffected)
            {
                activeScene.mainTilemap.FormatTile(tile.Key);
                action.ModifyTile(tile.Key, null, activeScene.mainTilemap.GetTileData(tile.Key));
            }
            
            CancelAction();
            ActionHistory.AddAction(currentAction);
        }

        //Functions
        private List<Vector2> GetEnclosedTiles()
        {
            List<Vector2> enclosedTiles = new List<Vector2>();
            ellipse.center = region.min + ((region.Size + Vector2.One) / 2);
            ellipse.radii.X = region.Size.X + 1;
            ellipse.radii.Y = region.Size.Y + 1;

            for (int x = (int)region.min.X; x <= (int)region.max.X + 1; x++)
            {
                for (int y = (int)region.min.Y; y <= (int)region.max.Y + 1; y++)
                {
                    if (ellipse.EnclosesPoint(new Vector2(x + 0.5f, y + 0.5f)))
                        enclosedTiles.Add(new Vector2(x, y));
                }
            }

            return enclosedTiles;
        }
    }
}