using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using Levels;
using System.Linq;
using Raylib_cs;
using MathExtras;
using Player;

namespace Engine
{
    public abstract class EditAction
    {
        public abstract void Undo(Scene scene);
        public abstract void Redo(Scene scene);

        public abstract override string ToString();
    }

    public class SetTilesAction : EditAction
    {
        public override void Undo(Scene scene)
        {
            foreach (KeyValuePair<Vector2, TileChange> pair in modifiedTiles)
            {
                scene.mainTilemap.SetTile((int)pair.Key.X, (int)pair.Key.Y, pair.Value.originalValue);
            }
        }
        public override void Redo(Scene scene)
        {
            foreach (KeyValuePair<Vector2, TileChange> pair in modifiedTiles)
            {
                scene.mainTilemap.SetTile((int)pair.Key.X, (int)pair.Key.Y, pair.Value.newValue);
            }
        }

        public override string ToString()
        {
            return "SetTilesAction: (" + modifiedTiles.Count + ")";
        }

        private readonly Dictionary<Vector2, TileChange> modifiedTiles = new Dictionary<Vector2, TileChange>();
        public void ModifyTile(Vector2 pos, byte? oldTile, byte? newTile)
        {
            if (modifiedTiles.ContainsKey(pos))
            {
                modifiedTiles[pos].newValue = newTile;
            }
            else
            {
                modifiedTiles[pos] = new TileChange(oldTile, newTile);
            }
        }
        public List<Vector2> GetModifiedTiles()
        {
            return modifiedTiles.Keys.ToList();
        }

        private class TileChange
        {
            public byte? originalValue;
            public byte? newValue;

            public TileChange(byte? _old, byte? _new)
            {
                originalValue = _old;
                newValue = _new;
            }
        }
    }
}