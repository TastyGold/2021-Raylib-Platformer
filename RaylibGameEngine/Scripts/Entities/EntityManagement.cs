using System;
using System.Numerics;
using System.Collections.Generic;
using MathExtras;
using Components;

namespace Levels
{
    public static partial class EntityManagement
    {
        //Interface for entity behavious in a scene
        public interface IScripted
        {
            public void RunBehaviour();
        }

        public interface IDrawable
        {
            public void Draw();
        }

        public interface IOilSort
        {
            public int OrderInLayer
            {
                get;
                set;
            }
        }

        //EntityIDs
        public static List<Type> EntityIDs = new List<Type>()
        {
            typeof(Player.PlayerCharacter),
            typeof(Coin),
            typeof(TriggerColldier),
            typeof(ParticleSystem),
            typeof(Spring)
        };

        //Entity class
        public abstract class Entity : IScripted, IOilSort
        {
            //References
            public Scene sceneReference;

            //Generic entity data
            private Transform2D _transform = new Transform2D();
            public Transform2D Transform { get => _transform; set => _transform = value; }
            public Vector2 Position
            {
                get => Transform.Position;
                set => Transform.Position = value;
            }

            public Vector2Int TilePosition => new Vector2Int((int)Position.X, (int)Position.Y);
            
            public int OrderInLayer { get; set; }

            //Loaded
            public bool isForceLoaded = false;
            public bool isLoaded = true;
            public bool isVisible = false;

            //Functions
            public abstract byte GetEntityID();
            public abstract Vector2 GetPositionOffset();

            //Methods
            public void Update(object sender, EventArgs e)
            {
                if (isForceLoaded || isLoaded)
                {
                    RunBehaviour();
                }
            }
            public abstract void RunBehaviour();
            public abstract void Draw();
            public virtual void DrawInEditor()
            {
                Draw();
            }
        }
    }

    public static class OrderInLayer
    {
        public static void InsertionSort(List<EntityManagement.Entity> list)
        {
            for (int i = 1; i < list.Count; i++)
            {
                for (int j = i-1; j >= 0; j--)
                {
                    if (list[j].OrderInLayer <= list[i].OrderInLayer)
                    {
                        list.Insert(j + 1, list[i]);
                        list.RemoveAt(i + 1);
                        j = -1; //done
                    }
                    else if (j == 0)
                    {
                        list.Insert(0, list[i]);
                        list.RemoveAt(i + 1);
                        j = -1; //done
                    }
                }
            }
        }
    }
}