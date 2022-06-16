using Engine;
using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;
using MathExtras;
using Components;

namespace Levels
{
    public static partial class EntityManagement
    {
        public abstract class Collider2D : Entity
        {
            //Collider data
            public Hitbox2D hitbox;

            public Vector2 minTileDist => (hitbox.Transform.Size + Vector2.One) / 2;

            //Movement detection
            private Vector2 lastPosition;
            public bool isStationary = false;
            public bool hasMoved = false;
            protected void HandleMoveDetection()
            {
                hasMoved = false;
                if (lastPosition != Position)
                {
                    hasMoved = true;
                }
                lastPosition = Position;
            }

            //References
            public List<List<Entity>> collisionCells = new List<List<Entity>>();

            //Methods
            public abstract override void RunBehaviour();
            public abstract void OnColliding(Collider2D e);
            public void Destroy()
            {
                
                for (int i = 0; i < collisionCells.Count; i++)
                {
                    if (collisionCells[i].Contains(this)) collisionCells[i].Remove(this);
                }
                sceneReference.TryRemoveEntity(this);
            }

            //Functions
            public List<Vector2> GetTileOverlaps(float padding) => GetTileOverlaps(hitbox, padding);
            public List<Vector2> GetTileOverlaps(Hitbox2D hb, float padding = 0f)
            {
                Vector2 cornerA = hb.Transform.Position - (Vector2.One * padding);
                Vector2 cornerB = hb.Transform.Position + hb.Transform.Size + (Vector2.One * padding);

                cornerA = new Vector2((int)cornerA.X, (int)cornerA.Y);
                cornerB = new Vector2((int)cornerB.X, (int)cornerB.Y);

                List<Vector2> tiles = new List<Vector2>();

                for (int x = (int)cornerA.X; x <= (int)cornerB.X; x++)
                {
                    for (int y = (int)cornerA.Y; y <= (int)cornerB.Y; y++)
                    {
                        tiles.Add(new Vector2(x, y));
                    }
                }

                return tiles;
            }
            public bool AnyTileOverlaps(Hitbox2D hb)
            {
                foreach (Vector2 v in GetTileOverlaps(hb, 0))
                {
                    if (sceneReference.mainTilemap.GetTileData(v).HasValue) return true;
                }
                return false;
            }

            public Vectex GetHitboxVectex()
            {
                return new Vectex(hitbox.Transform.Position, hitbox.Transform.Position + hitbox.Transform.Size);
            }
            public Vectex GetHitboxVectexPadded(float padding)
            {
                return new Vectex(hitbox.Transform.Position - (Vector2.One * padding), hitbox.Transform.Position + hitbox.Transform.Size + (Vector2.One * padding));
            }

            public bool IsOverlapping(Vector2 tile, out float xDist, out float yDist)
            {
                Vector2 tileCenter = tile + new Vector2(0.5f);
                xDist = tileCenter.X - (hitbox.X + (hitbox.SizeX * 0.5f));
                yDist = tileCenter.Y - (hitbox.Y + (hitbox.SizeY * 0.5f));

                return hitbox.IsOverlapping(new Hitbox2D(tile, Vector2.One));
            }
        }

        public struct Hitbox2D
        {
            //Data
            public Transform2D Transform { get; set; }

            //Properties
            public float X { get => Transform.Position.X; set => Transform.Position = new Vector2(value, Transform.Position.Y); }
            public float Y { get => Transform.Position.Y; set => Transform.Position = new Vector2(Transform.Position.X, value); }

            public float SizeX { get => Transform.Size.X; set => Transform.Size = new Vector2(value, Transform.Size.Y); }
            public float SizeY { get => Transform.Size.Y; set => Transform.Size = new Vector2(Transform.Size.X, value); }

            //Functions
            public bool IsOverlapping(Hitbox2D b)
            {
                return !(X > b.X + b.SizeX || X + SizeX < b.X ||
                    Y > b.Y + b.SizeY || Y + SizeY < b.Y);
            }

            //Rendering
            public void Draw(Color color)
            {
                Rectangle r = Rendering.GetScreenRect(Transform.Position, Transform.Size);
                Raylib.DrawRectangleLinesEx(r, 1, color);
                Rendering.CountDrawCallSimple();
            }

            //Intialisation
            public Hitbox2D(float x, float y, float xSize, float ySize)
            {
                Transform = new Transform2D()
                {
                    Position = new Vector2(x, y),
                    Size = new Vector2(xSize, ySize),
                };
            }
            public Hitbox2D(Vector2 position, Vector2 size)
            {
                Transform = new Transform2D()
                {
                    Position = position,
                    Size = size,
                };
            }
            public Hitbox2D(Transform2D parent, Vector2 localPosition, Vector2 size)
            {
                Transform = new Transform2D()
                {
                    Pivot = parent,
                    LocalPosition = localPosition,
                    Size = size,
                };
            }
        }
    }
}