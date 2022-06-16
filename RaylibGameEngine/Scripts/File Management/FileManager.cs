using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Resources;
using MathExtras;

namespace Levels
{
    public static class FileManager
    {
#if DEBUG
        public const string mainDir = "..\\..\\..\\";
#else
        public const string mainDir = "\\";
#endif
        
        public const string levelDataDir = mainDir + "LevelData\\";
        public const string assetsDir = mainDir + "Assets\\";

        /// <summary>
        /// Writes a level instance to a .lvl file
        /// </summary>
        /// <param name="level">Level instance to be saved to the file</param>
        /// <param name="path">The path relative to the LevelData folder. Eg: "TestLevel.lvl" or "LevelFolder\\TestLevel.lvl"</param>
        /// <returns></returns>
        public static void SaveLevelToFile(Level level, string path)
        {
            Console.WriteLine("");
            Console.WriteLine($"LVLFILE: Saving level file: {path}");

            using (BinaryWriter outputFile = new BinaryWriter(File.Open(Path.Combine(levelDataDir, path), FileMode.Create)))
            {
                //Metadata - TBD

                //Scenes
                outputFile.Write(level.NumberOfScenes);
                Console.WriteLine($"LVLFILE: Number of scenes: {level.NumberOfScenes}");

                for (int i = 0; i < level.NumberOfScenes; i++)
                {
                    //Tilemap
                    outputFile.WriteTilemapCompressed(level.GetScene(i).mainTilemap);

                    //Semisolids
                    outputFile.WriteSemisolids(level.GetScene(i).semisolids);

                    //Entities
                    outputFile.WriteEntities(level.GetScene(i).GetEntityList());
                }
            }

            Console.WriteLine($"LVLFILE: Finished saving level to file {path}");
            Console.WriteLine("");
        }
        public static Level ReadLevelFromFile(string path)
        {
            Console.WriteLine("");
            Console.WriteLine($"LVLFILE: Reading level from file: {path}");

            if (!File.Exists(Path.Combine(levelDataDir, path)))
            {
                Console.WriteLine("LVLFILE: Level does not exist. Creating new: levelDataPath");
                Console.WriteLine("");
                return Level.BlankScene;
            }

            Level levelData = new Level();

            using (BinaryReader binaryFile = new BinaryReader(File.Open(Path.Combine(levelDataDir, path), FileMode.Open)))
            {
                ReadLevelData(binaryFile, ref levelData);
            }

            Console.WriteLine($"LVLFILE: Finished reading level: {path}");
            Console.WriteLine("");
            return levelData;
        }
        public static Level ReadLevelFromResource(byte[] levelDataArray)
        {
            Console.WriteLine("");
            Console.WriteLine($"LVLFILE: Reading level from resource");

            Level levelData = new Level();
            MemoryStream stream = new MemoryStream(levelDataArray);

            using (BinaryReader binaryFile = new BinaryReader(stream))
            {
                ReadLevelData(binaryFile, ref levelData);
            }

            Console.WriteLine($"LVLFILE: Finished reading resource");
            Console.WriteLine("");
            return levelData;
        }

        private static void ReadLevelData(BinaryReader reader, ref Level levelData)
        {
            //Metadata - TBD

            //Scenes
            int numberOfScenes = reader.ReadByte();
            Console.WriteLine($"LVLFILE: Number of scenes: {numberOfScenes}");

            for (int i = 0; i < numberOfScenes; i++)
            {
                byte?[,] _mainTilemap = reader.ReadTilemapCompressed();
                List<Semisolid> _semisolids = reader.ReadSemisolids();
                List<EntityManagement.Entity> _entities = reader.ReadEntities();

                levelData.AddScene(new Scene()
                {
                    mainTilemap = new Tilemap(_mainTilemap),
                    semisolids = _semisolids,
                });
                levelData.SetSceneEntityList(i, _entities);
            }
        }

        private static void WriteTilemapCompressed(this BinaryWriter outputFile, Tilemap map)
        {
            //Dimensions
            int xSize = map.TilemapWidth;
            int ySize = map.TilemapHeight;

            Console.WriteLine($"LVLFILE: Writing tilemap. Size <{xSize}, {ySize}>");

            outputFile.Write((ushort)xSize);
            outputFile.Write((ushort)ySize);

            byte[] tilemapBitmask = map.GetTilemapBitmask();
            outputFile.Write(tilemapBitmask);

            byte[] tileIDs = map.GetTilemapDataBitmasked();
            outputFile.Write(tileIDs);

            Console.WriteLine("LVLFILE: Finished writing tilemap");
        }
        private static byte?[,] ReadTilemapCompressed(this BinaryReader binaryFile)
        {
            //Dimensions
            ushort xSize = binaryFile.ReadUInt16();
            ushort ySize = binaryFile.ReadUInt16();

            Console.WriteLine($"LVLFILE: Reading tilemap. Size: <{xSize}, {ySize}>");

            BitArray tileMask = new BitArray(binaryFile.ReadBytes(xSize * ySize / 8));

            byte?[,] bytes = new byte?[xSize, ySize];

            byte[] tileIDs = binaryFile.ReadBytes(BlackMagic.GetCardinality(tileMask));

            for (int x = 0, i = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    if (tileMask.Get(y + (x * ySize)))
                    {
                        bytes[x, y] = tileIDs[i];
                        i++;
                    }
                }
            }

            Console.WriteLine($"LVLFILE: Finished reading tilemap");
            return bytes;
        }

        private static void WriteSemisolids(this BinaryWriter outputFile, List<Semisolid> semisolids)
        {
            Console.WriteLine($"LVLFILE: Saving semisolids. Amount: {semisolids.Count}");

            //Amount
            outputFile.Write((ushort)semisolids.Count);

            for (int i = 0; i < semisolids.Count; i++)
            {
                //Write semisolid to file
                outputFile.Write((byte)semisolids[i].themeIndex);
                outputFile.Write((ushort)semisolids[i].x);
                outputFile.Write((ushort)semisolids[i].y);
                outputFile.Write((byte)semisolids[i].width);
                outputFile.Write((byte)semisolids[i].height);
                outputFile.Write((bool)semisolids[i].hasSurface);
            }

            Console.WriteLine("LVLFILE: Finished saving semisolids");
        }
        private static List<Semisolid> ReadSemisolids(this BinaryReader binaryFile)
        {
            Console.WriteLine("LVLFILE: Reading semisolids");
            List<Semisolid> semisolids = new List<Semisolid>();

            //Amount
            ushort amount = binaryFile.ReadUInt16();

            for (int i = 0; i < amount; i++)
            {
                //Read semisolid
                byte themeIndex = binaryFile.ReadByte();
                ushort x = binaryFile.ReadUInt16();
                ushort y = binaryFile.ReadUInt16();
                byte width = binaryFile.ReadByte();
                byte height = binaryFile.ReadByte();
                bool hasSurface = binaryFile.ReadBoolean();

                semisolids.Add(new Semisolid(x, y, width, height, hasSurface, themeIndex));
            }

            Console.WriteLine($"LVLFILE: Finished reading semisolids. Amount: {amount}");
            return semisolids;
        }

        private static void WriteEntities(this BinaryWriter outputFile, List<EntityManagement.Entity> entities)
        {
            Console.WriteLine($"LVLFILE: Saving entities. Amount: {entities.Count}");

            //Amount
            outputFile.Write((ushort)entities.Count);

            for (int i = 0; i < entities.Count; i++)
            {
                //Write entity to file
                outputFile.Write(entities[i].GetEntityID());
                outputFile.Write((ushort)entities[i].Position.X);
                outputFile.Write((ushort)entities[i].Position.Y);
            }

            Console.WriteLine($"LVLFILE: Finished saving entites");
        }
        private static List<EntityManagement.Entity> ReadEntities(this BinaryReader binaryFile)
        {
            Console.WriteLine($"LVLFILE: Reading entites");
            List<EntityManagement.Entity> entities = new List<EntityManagement.Entity>();

            //Amount
            ushort amount = binaryFile.ReadUInt16();

            for (int i = 0; i < amount; i++)
            {
                //Read entity
                byte id = binaryFile.ReadByte();
                ushort x = binaryFile.ReadUInt16();
                ushort y = binaryFile.ReadUInt16();

                EntityManagement.Entity e = (EntityManagement.Entity)Activator.CreateInstance(EntityManagement.EntityIDs[id]);
                e.Position = new System.Numerics.Vector2(x, y) + e.GetPositionOffset();

                entities.Add(e);
            }

            Console.WriteLine($"LVLFILE: Finished reading entities. Amount: {entities.Count}");
            return entities;
        }

        private static void WriteDecor(this BinaryWriter outputFile, List<DecorObject> decorObjects)
        {
            Console.WriteLine($"LVLFILE: Saving decor. Amount: {decorObjects.Count}");
            outputFile.Write((ushort)decorObjects.Count);

            for (int i = 0; i < decorObjects.Count; i++)
            {
                //outputFile.Write(decorObjects[i].)
            }
        }
    }
}