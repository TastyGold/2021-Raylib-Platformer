using System;
using System.Collections;

namespace Levels
{
    public static class BetterTiling
    {
        public static byte GetTilingCode(byte?[,] map, int x, int y)
        {
            byte code = 0b0000000;

            // Bit Order
            // 7 6 5
            // 4 . 3
            // 2 1 0

            if (TileHasValue(map, x - 1, y + 1)) code += 0b10000000;
            if (TileHasValue(map, x, y + 1))     code += 0b1000000;
            if (TileHasValue(map, x + 1, y + 1)) code += 0b100000;
            if (TileHasValue(map, x - 1, y))     code += 0b10000;
            if (TileHasValue(map, x + 1, y))     code += 0b1000;
            if (TileHasValue(map, x - 1, y - 1)) code += 0b100;
            if (TileHasValue(map, x, y - 1))     code += 0b10;
            if (TileHasValue(map, x + 1, y - 1)) code += 0b1;

            return code;
        }

        public static bool TileHasValue(byte?[,] map, int x, int y)
        {
            return !(x < 0 || x >= map.GetLength(0) || y < 0 || y >= map.GetLength(1)) ? map[x, y].HasValue : true;
        }

        public static int GetTileID(byte code, TileRuleset rule)
        {
            byte edgeMask = 0b01011010;
            byte edges = (byte)(code & edgeMask);

            for (int i = 0; i < rule.rules.Length; i++)
            {
                if (edges == (rule.rules[i] & edgeMask))
                {
                    byte importanceMask = edgeMask;

                    bool up = ((edges >> 6) & 1) == 1;
                    bool left = ((edges >> 4) & 1) == 1;
                    bool right = ((edges >> 3) & 1) == 1;
                    bool down = ((edges >> 1) & 1) == 1;

                    if (up)
                    {
                        if (left)  importanceMask += 0b10000000;
                        if (right) importanceMask += 0b00100000;
                    }
                    if (down)
                    {
                        if (left)  importanceMask += 0b00000100;
                        if (right) importanceMask += 0b00000001;
                    }

                    if ((code & importanceMask) == (rule.rules[i] & importanceMask))
                    {
                        return i;
                    }
                }
            }

            throw new Exception($"Code {code} not found.");
        }

        public class TileRuleset
        {
            public byte[] rules;

            public TileRuleset(byte[] rules)
            {
                this.rules = rules;
            }

            public static TileRuleset Tileset8x6 = new TileRuleset(new byte[47]
            {
                0b00001011, 0b00011111, 0b00010110, 0b00000010, 0b00011110, 0b00011011, 0b00001010, 0b00010010,
                0b01101011, 0b11111111, 0b11010110, 0b01000010, 0b11011000, 0b01111000, 0b01001000, 0b01010000,
                0b01101000, 0b11111000, 0b11010000, 0b01000000, 0b01101010, 0b11010010, 0b00011010, 0b01010010,
                0b00001000, 0b00011000, 0b00010000, 0b00000000, 0b01001011, 0b01010110, 0b01001010, 0b01011000,
                0b11111110, 0b11111011, 0b11011110, 0b11111010, 0b11011010, 0b01111010, 0b01111110, 0b11011011,
                0b11011111, 0b01111111, 0b01011111, 0b01111011, 0b01011110, 0b01011011, 0b01011010, 
            });
        }
    }
}