using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using Engine;
using Raylib_cs;
using MathExtras;
using Player;
using static Levels.EntityManagement;

namespace Levels
{
    public class Level
    {
        //Scene collection
        private List<Scene> scenes;
        private int activeSceneIndex = 0;
        public int GetActiveSceneIndex => activeSceneIndex;
        public Scene ActiveScene => scenes.Count > 0 ? scenes[activeSceneIndex] : null;
        public byte NumberOfScenes => (byte)scenes.Count;
        public void SetSceneEntityList(int index, List<Entity> e)
        {
            scenes[index].SetEntityList(e);
        }

        //Scene Control
        public void AddScene(Scene newScene)
        {
            scenes.Add(newScene);
            newScene.collisionMap = new UniformGrid(new Vector2Int(newScene.mainTilemap.TilemapWidth, newScene.mainTilemap.TilemapHeight));
        }
        public Scene GetScene(int sceneIndex)
        {
            return scenes[sceneIndex];
        }
        public void DeleteActiveScene(Level level)
        {
            level.scenes.RemoveAt(activeSceneIndex);
            activeSceneIndex = 0;
        }

        //Scene Runtime
        public void DrawActiveScene()
        {
            switch (EntryPoint.startupMode)
            {
                case EntryPoint.LaunchMode.Editor: //realtime tilemap
                    ActiveScene.SemisolidsDescending.DrawAll();
                    ActiveScene.mainTilemap.DrawAllTiles();
                    ActiveScene.GetEntityList().ForEach(e => e.DrawInEditor());
                    break;
                case EntryPoint.LaunchMode.Gameplay: //baked tilemap
                    ActiveScene.DrawMidground();
                    ActiveScene.GetEntityList().ForEach(e => { if (e.isVisible) e.Draw(); });
                    ActiveScene.DrawForeground();
                    break;
            }
        }
        public void LoadGameplayScene(int sceneIndex)
        {
            if (scenes.Count < sceneIndex + 1)
            {
                throw new IndexOutOfRangeException($"Scene [{sceneIndex}] does not exist");
            }
            else
            {
                activeSceneIndex = sceneIndex;
            }
        }
        public void LoadEditorScene(int sceneIndex)
        {
            if (scenes.Count < sceneIndex + 1)
            {
                throw new IndexOutOfRangeException($"Scene [{sceneIndex}] does not exist");
            }
            else
            {
                activeSceneIndex = sceneIndex;
            }
        }

        //Initialisation
        public Level()
        {
            this.scenes = new List<Scene>();
        }
        public Level(List<Scene> scenes)
        {
            this.scenes = scenes;
        }
        public static Level Empty = new Level()
        {
            scenes = new List<Scene> { new Scene() }
        };
        public static Level BlankScene = new Level()
        {
            scenes = new List<Scene>()
            {
                new Scene()
            }
        };
    }

    public class Scene
    {
        //World data
        public Tilemap mainTilemap;
        public List<Semisolid> semisolids = new List<Semisolid>();
        public List<Semisolid> SemisolidsDescending = new List<Semisolid>();
        public void RecalculateSemisolidOrdering()
        {
            if (semisolids.Count > 0)
            {
                SemisolidsDescending = semisolids.OrderByDescending(o => ((o.y + o.height - 1) * 256) + o.width).ToList();
            }
            else
            {
                SemisolidsDescending = new List<Semisolid>();
            }
        }
        public List<DecorObject> decorObjects = new List<DecorObject>();

        public RenderTexture2D bakedMidgroundTexture;
        public RenderTexture2D bakedForegroundTexture;
        public RenderTexture2D bakedTilemapOverlay;
        public bool hasBakedMidground = false;
        public bool hasBakedForeground = false;
        public bool hasBakedOverlay = false;
        public void BakeTilemapToTexture()
        {
            if (hasBakedForeground) Raylib.UnloadRenderTexture(bakedForegroundTexture);
            bakedForegroundTexture = mainTilemap.BakeTilemapTexture();
            hasBakedForeground = true;
        }
        public void BakeTilemapOverlay()
        {
            if (hasBakedOverlay) Raylib.UnloadRenderTexture(bakedTilemapOverlay);
            bakedTilemapOverlay = Raylib.LoadRenderTexture(mainTilemap.TilemapWidth, mainTilemap.TilemapHeight);
            Raylib.BeginTextureMode(bakedTilemapOverlay);

            for (int x = 0; x < mainTilemap.TilemapWidth; x++)
            {
                for (int y = 0; y < mainTilemap.TilemapHeight; y++)
                {
                    if (mainTilemap.GetTileData(x, y).HasValue)
                    {
                        Raylib.DrawPixel(x, y, Color.LIGHTGRAY);
                    }
                }
            }

            Raylib.EndTextureMode();
            hasBakedOverlay = true;
        }
        public void BakeMidgroundTexture()
        {
            if (hasBakedMidground) Raylib.UnloadRenderTexture(bakedMidgroundTexture);

            RenderTexture2D tex = Raylib.LoadRenderTexture(mainTilemap.TilemapWidth * 16, mainTilemap.TilemapHeight * 16);
            Raylib.BeginTextureMode(tex);
            RecalculateSemisolidOrdering();
            SemisolidsDescending.ForEach(semisolid => semisolid.DrawToTexture());
            decorObjects.ForEach(d => d.DrawToTexture());
            Raylib.EndTextureMode();
            bakedMidgroundTexture = tex;

            hasBakedMidground = true;
        }
        public void BakeLevelTextures()
        {
            BakeMidgroundTexture();
            BakeTilemapToTexture();
            BakeTilemapOverlay();
        }
        public void DrawMidground()
        {
            Raylib.DrawTextureEx(bakedMidgroundTexture.texture, Vect.Down * bakedMidgroundTexture.texture.height * Screen.pixelScale, 0, Screen.pixelScale, Color.WHITE);
            Rendering.CountDrawCall(bakedMidgroundTexture.texture.id);
        }
        public void DrawForeground()
        {
            Raylib.DrawTextureEx(bakedForegroundTexture.texture, Vect.Down * bakedForegroundTexture.texture.height * Screen.pixelScale, 0, Screen.pixelScale, Color.WHITE);
            Rendering.CountDrawCall(bakedForegroundTexture.texture.id);
        }
        public void DrawOverlay()
        {
            Raylib.DrawTextureEx(bakedTilemapOverlay.texture, Vect.Down * bakedTilemapOverlay.texture.height * Screen.pixelScale * 16, 0, Screen.pixelScale * 16, Color.WHITE);
            Rendering.CountDrawCall(bakedTilemapOverlay.texture.id);
        }

        //Entity data
        public PlayerCharacter playerReference;
        public bool hasPlayer = false;
        public void AddPlayer(Vector2 position)
        {
            PlayerCharacter p = new PlayerCharacter();
            playerReference = p;
            AddEntity(p, position);
            hasPlayer = true;
        }

        private readonly List<Entity> _entityList = new List<Entity>();
        public List<Entity> GetEntityList() { return _entityList; }
        public void SetEntityList(List<Entity> value)
        {
            for (int i = 0; i < value.Count; i++)
            {
                AddEntity(value[i], value[i].Position);
            }
        }

        public bool IsOccupied(Vector2Int position)
        {
            foreach(Entity e in GetEntityList())
            {
                if (e.Position.ToVector2Int().Equals(position))
                {
                    return true;
                }
            }
            return false;
        }
        public bool IsOccupied(Vector2Int position, int id)
        {
            foreach (Entity e in GetEntityList())
            {
                if (e.GetEntityID() == id && e.Position.ToVector2Int().Equals(position))
                {
                    return true;
                }
            }
            return false;
        }

        public void AddEntity(Entity newEntity, Vector2 position)
        {
            newEntity.Position = position;
            UpdateEvent += newEntity.Update;
            GetEntityList().Add(newEntity);
            newEntity.sceneReference = this;
            collisionMap.AddEntity(newEntity);
        }
        public void TryRemoveEntity(Entity e)
        {
            if (_entityList.Contains(e)) _entityList.Remove(e);
            else throw new ArgumentNullException($"{e} not found in _entityList");
        }
        public void RemoveEntityAt(Vector2Int position, int id = -1)
        {
            foreach (Entity e in _entityList)
            {
                if (e.Position.ToVector2Int().Equals(position) &&
                    (id == -1 || id == e.GetEntityID()))
                {
                    _entityList.Remove(e);
                    return;
                }
            }
        }

        public void SortOrderInLayer()
        {
            OrderInLayer.InsertionSort(_entityList);
        }

        //Collision Mapping
        public UniformGrid collisionMap;

        //Behaviour
        public void Update()
        {
            UpdateEvent(this, EventArgs.Empty);
        }
        public event EventHandler UpdateEvent;
        protected virtual void OnUpdate(EventArgs e)
        {
            UpdateEvent?.Invoke(this, e);
        }

        //Initialisation
        public Scene()
        {
            mainTilemap = new Tilemap();
        }
    }
}