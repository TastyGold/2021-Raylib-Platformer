using Raylib_cs;
using System;
using System.Numerics;
using System.Collections.Generic;
using MathExtras;
using Levels;
using System.Linq;
using static Levels.EntityManagement;

namespace Engine
{
    public class UniformGrid
    {
        //Data
        private const int gridCellSize = 8;
        private const float iCellSize = 1f / (float)gridCellSize;
        private readonly List<Entity>[,] gridCells = new List<Entity>[64, 16];

        private Vector2Int loadedRegionPosition = new Vector2Int(0, 0);
        private Vector2Int loadedRegionSize = new Vector2Int(5, 5);
        private bool IsCellLoaded(int x, int y)
        {
            return !(
                x < loadedRegionPosition.X || x > loadedRegionPosition.X + loadedRegionSize.X ||
                y < loadedRegionPosition.Y || y > loadedRegionPosition.Y + loadedRegionSize.Y
                );
        }

        public void SetLoadedRegion(Vector2Int start, Vector2Int end)
        {
            loadedRegionPosition = start;
            loadedRegionSize = end - start;
        }
        public static Vector2Int GetGridCell(Vector2 position)
        {
            return (position * iCellSize).ToVector2Int();
        }

        //Check for collisions on objects that have moved
        public void UpdateEntityCells()
        {
            for (int x = 0; x < gridCells.GetLength(0); x++)
            {
                for (int y = 0; y < gridCells.GetLength(1); y++)
                {
                    if (gridCells[x, y].Count != 0)
                    {
                        for (int i = 0; i < gridCells[x, y].Count; i++)
                        {
                            if (IsCellLoaded(x, y))
                            { 
                                gridCells[x, y][i].isLoaded = true;
                                gridCells[x, y][i].isVisible = true;
                            }
                            else
                            {
                                gridCells[x, y][i].isLoaded = false;
                                gridCells[x, y][i].isVisible = false;
                            }

                            if (gridCells[x, y][i] is Collider2D col)
                            {
                                if (!col.isStationary && col.hasMoved)
                                {
                                    MoveEntity(col, new Vector2Int(x, y));
                                    CheckForCollisions(col, new Vector2Int(x, y));
                                }
                            }
                        }
                    }
                }
            }
        }
        public void CheckForCollisions(Collider2D e, Vector2Int cell)
        {
            if (gridCells[cell.X, cell.Y].Count > 1)
            {
                for (int i = 0; i < gridCells[cell.X, cell.Y].Count; i++)
                {
                    if (gridCells[cell.X, cell.Y][i] is Collider2D comparison)
                    {
                        if (!e.Equals(comparison))
                        {
                            //Raylib.DrawLineEx((Vector2)Rendering.GetScreenPosition(e.Position * Vect.FlipY), (Vector2)Rendering.GetScreenPosition(comparison.Position * Vect.FlipY), 1 + (1 / Gameplay.playerCamera.zoom), Color.MAGENTA);

                            if (e.hitbox.IsOverlapping(comparison.hitbox))
                            {
                                e.OnColliding(comparison);
                                comparison.OnColliding(e);
                            }
                        }
                    }
                }
            }
        }

        //Manually adds an entity to the entity map
        public void AddEntity(Entity newEntity)
        {
            if (!(newEntity is Collider2D e))
            {
                Vector2Int cell = GetGridCell(newEntity.Position);
                gridCells[cell.X, cell.Y].Add(newEntity);
            }
            else
            {
                Vector2Int bottomLeftCell = (e.hitbox.Transform.Position * iCellSize).ToVector2Int();
                Vector2Int topRightCell = ((e.hitbox.Transform.Position + e.hitbox.Transform.Size) * iCellSize).ToVector2Int();

                Vector2Int startCell = new Vector2Int(Math.Max(bottomLeftCell.X, 0), Math.Max(bottomLeftCell.Y, 0));
                Vector2Int endCell = new Vector2Int(Math.Min(topRightCell.X, gridCells.GetLength(0) - 1), Math.Min(topRightCell.Y, gridCells.GetLength(1) - 1));

                for (int x = startCell.X; x <= endCell.X; x++)
                {
                    for (int y = startCell.Y; y <= endCell.Y; y++)
                    {
                        if (!gridCells[x, y].Contains(e))
                        {
                            gridCells[x, y].Add(e);
                            e.collisionCells.Add(gridCells[x, y]);
                        }
                    }
                }
            }
        }

        //Recalculates entity's habitation
        private void MoveEntity(Collider2D e, Vector2Int lastCell)
        {
            Vector2Int bottomLeftCell = (e.hitbox.Transform.Position * iCellSize).ToVector2Int();
            Vector2Int topRightCell = ((e.hitbox.Transform.Position + e.hitbox.Transform.Size) * iCellSize).ToVector2Int();

            Vector2Int startCell = new Vector2Int(Math.Max(bottomLeftCell.X, 0), Math.Max(bottomLeftCell.Y, 0));
            Vector2Int endCell = new Vector2Int(Math.Min(topRightCell.X, gridCells.GetLength(0) - 1), Math.Min(topRightCell.Y, gridCells.GetLength(1) - 1));

            bool leftLastCell = true;
            for (int x = startCell.X; x <= endCell.X; x++)
            {
                for (int y = startCell.Y; y <= endCell.Y; y++)
                {
                    if (lastCell.X == x && lastCell.Y == y) leftLastCell = false;
                    if (!gridCells[x, y].Contains(e))
                    {
                        gridCells[x, y].Add(e);
                        e.collisionCells.Add(gridCells[x, y]);
                    }
                }
            }
            if (leftLastCell && !IsOutOfBounds(startCell, endCell, bottomLeftCell, topRightCell))
            {
                gridCells[lastCell.X, lastCell.Y].Remove(e);
                e.collisionCells.Remove(gridCells[lastCell.X, lastCell.Y]);
            }
        }
        private bool IsOutOfBounds(Vector2Int startCell, Vector2Int endCell, Vector2Int bottomLeftCell, Vector2Int topRightCell)
        {
            return startCell.X > topRightCell.X || startCell.Y > topRightCell.Y || endCell.X < bottomLeftCell.X || endCell.Y < bottomLeftCell.Y;
        }

        //Rendering
        public void DrawGrid(int lineWidth, bool highlightOccupiedCells = false)
        {
            for (int x = 0; x < gridCells.GetLength(0); x++)
            {
                for (int y = 0; y < gridCells.GetLength(1); y++)
                {
                    if (IsCellLoaded(x, y))
                    {
                        Rectangle r = Rendering.GetScreenRect(x * gridCellSize, y * gridCellSize, gridCellSize, gridCellSize);

                        Raylib.DrawRectangleLinesEx(r, lineWidth, highlightOccupiedCells && gridCells[x, y].Count != 0 ? Color.RED : Color.GRAY);
                    }
                }
            }
        }

        //Initialisation
        public UniformGrid(Vector2Int levelSize)
        {
            int x = levelSize.X / gridCellSize;
            int y = levelSize.Y / gridCellSize;
            if (levelSize.X % gridCellSize != 0) x++;
            if (levelSize.Y % gridCellSize != 0) y++;

            gridCells = new List<Entity>[x, y];

            for (int xi = 0; xi < x; xi++)
            {
                for (int yi = 0; yi < y; yi++)
                {
                    gridCells[xi, yi] = new List<Entity>();
                }
            }
        }
    }
}