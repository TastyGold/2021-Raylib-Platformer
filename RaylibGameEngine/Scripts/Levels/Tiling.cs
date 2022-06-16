using System;

namespace Levels
{
    public static class Tiling
    {
        public static bool[,] GetTilingInfo(byte?[,] map, int x, int y)
        {
            bool[,] checks = new bool[3, 3];
            
            //Left side
            if (x > 0)
            {
                if (y > 0)
                {
                    if (map[x - 1, y - 1].HasValue)
                        checks[0, 2] = true;
                    else
                        checks[0, 2] = false;
                }
                else
                {
                    checks[0, 2] = true;
                }
                if (map[x - 1, y].HasValue)
                    checks[0, 1] = true;
                else
                    checks[0, 1] = false;
                if (y < map.GetLength(1) - 1)
                {
                    if (map[x - 1, y + 1].HasValue)
                        checks[0, 0] = true;
                    else
                        checks[0, 0] = false;
                }
                else
                {
                    checks[0, 0] = true;
                }
            }
            else
            {
                checks[0, 0] = true;
                checks[0, 1] = true;
                checks[0, 2] = true;
            }

            //Center
            if (y > 0)
            {
                if (map[x, y - 1].HasValue)
                    checks[1, 2] = true;
                else
                    checks[1, 2] = false;
            }
            else
            {
                checks[1, 2] = true;
            }
            if (map[x , y].HasValue)
                checks[1, 1] = true;
            else
                checks[1, 1] = false;
            if (y < map.GetLength(1) - 1)
            {
                if (map[x, y + 1].HasValue)
                    checks[1, 0] = true;
                else
                    checks[1, 0] = false;
            }
            else
            {
                checks[1, 0] = true;
            }

            //Right side
            if (x < map.GetLength(0) - 1)
            {
                if (y > 0)
                {
                    if (map[x + 1, y - 1].HasValue)
                        checks[2, 2] = true;
                    else
                        checks[2, 2] = false;
                }
                else
                {
                    checks[2, 2] = true;
                }
                if (map[x + 1, y].HasValue)
                    checks[2, 1] = true;
                else
                    checks[2, 1] = false;
                if (y < map.GetLength(1) - 1)
                {
                    if (map[x + 1, y + 1].HasValue)
                        checks[2, 0] = true;
                    else
                        checks[2, 0] = false;
                }
                else
                {
                    checks[2, 0] = true;
                }
            }
            else
            {
                checks[2, 0] = true;
                checks[2, 1] = true;
                checks[2, 2] = true;
            }

            return checks;
        }

        public static string GetTilingCode(byte?[,] map, int x, int y)
        {
            bool[,] checks = GetTilingInfo(map, x, y);
            string output = "";
            for (int yr = 0; yr < 3; yr++)
            {
                for (int xr = 0; xr < 3; xr++)
                {
                    output += checks[xr, yr] ? "+" : "-";
                }
            }
            return output;
        }

        public class Format
        {
            public Rule[,] rules;

            public Format(Rule[,] rules)
            {
                this.rules = rules;
            }
        }

        public static Format Tileset6x4 = new Format(new Rule[6, 4]
            {
                //Column 1
                { 
                    new Rule(".-.-++.+."),
                    new Rule(".+.-++.+."),
                    new Rule(".+.-++.-."),
                    new Rule(".-.-++.-."),
                },

                //Column 2
                {
                    new Rule(".-.+++.+."),
                    new Rule("+++++++++"),
                    new Rule(".+.+++.-."),
                    new Rule(".-.+++.-."),
                },

                //Column 3
                {
                    new Rule(".-.++-.+."),
                    new Rule(".+.++-.+."),
                    new Rule(".+.++-.-."),
                    new Rule(".-.++-.-."),
                },

                //Column 4
                {
                    new Rule(".-.-+-.+."),
                    new Rule(".+.-+-.+."),
                    new Rule(".+.-+-.-."),
                    new Rule(".-.-+-.-."),
                },

                //Column 5
                {
                    new Rule(".+.+++.+-"),
                    new Rule(".+-+++.+."),
                    new Rule(".+-+++.+-"),
                    new Rule("-+-+++.+."),
                },

                //Column 6
                {
                    new Rule(".+.+++-+."),
                    new Rule("-+.+++.+."),
                    new Rule(".+.+++-+-"),
                    new Rule("-+.+++-+."),
                }
            });
        public static Format Tileset12x4 = new Format(new Rule[12, 4]
            {
                //Column 1
                {
                    new Rule(".-.-++.++"), //1
                    new Rule(".++-++.++"), //2
                    new Rule(".++-++.-."), //3
                    new Rule(".-.-++.-."), //4
                },

                //Column 2
                {
                    new Rule(".-.++++++"), //1
                    new Rule("+++++++++"), //2
                    new Rule("++++++.-."), //3
                    new Rule(".-.+++.-."), //4
                },

                //Column 3
                {
                    new Rule(".-.++-++."), //1
                    new Rule("++.++-++."), //2
                    new Rule("++.++-.-."), //3
                    new Rule(".-.++-.-."), //4
                },

                //Column 4
                {
                    new Rule(".-.-+-.+."), //1
                    new Rule(".+.-+-.+."), //2
                    new Rule(".+.-+-.-."), //3
                    new Rule(".-.-+-.-."), //4
                },

                //Column 5
                {
                    new Rule("++++++++-"), //1
                    new Rule("++-++++++"), //2
                    new Rule("++-+++++-"), //3
                    new Rule("-+-++++++"), //4
                },

                //Column 6
                {
                    new Rule("++++++-++"), //1
                    new Rule("-++++++++"), //2
                    new Rule("++++++-+-"), //3
                    new Rule("-+++++-++"), //4
                },

                //Column 7
                {
                    new Rule(".-.-++.+-"), //1
                    new Rule(".+--++.-."), //2
                    new Rule(".+--++.+-"), //3
                    new Rule("-+-+++.-."), //4
                },

                //Column 8
                {
                    new Rule(".-.++--+."), //1
                    new Rule("-+.++-.-."), //2
                    new Rule(".-.+++-+-"), //3
                    new Rule("-+.++--+."), //4
                },

                //Column 9
                {
                    new Rule(".++-++.+-"), //1
                    new Rule(".+--++.++"), //2
                    new Rule(".-.+++++-"), //3
                    new Rule("++-+++.-."), //4
                },

                //Column 10
                {
                    new Rule("++.++--+."), //1
                    new Rule("-+.++-++."), //2
                    new Rule(".-.+++-++"), //3
                    new Rule("-+++++.-."), //4
                },

                //Column 11
                {
                    new Rule("-+-+++-++"), //1
                    new Rule("-+++++-+-"), //2
                    new Rule("-+++++++-"), //3
                    new Rule("-+-+++-+-"), //4
                },

                //Column 12
                {
                    new Rule("-+-+++++-"), //1
                    new Rule("++-+++-+-"), //2
                    new Rule("++-+++-++"), //3
                    new Rule("---------"), //4
                },
            });
        public static Format Tileset8x6 = new Format(new Rule[8, 6]
        {
                //Column 1
                {
                    new Rule(".-.-++.++"), //1
                    new Rule(".++-++.++"), //2
                    new Rule(".++-++.-."), //3
                    new Rule(".-.-++.-."), //4
                    new Rule("++++++++-"), //5
                    new Rule("++-++++++"), //6
                },

                //Column 2
                {
                    new Rule(".-.++++++"), //1
                    new Rule("+++++++++"), //2
                    new Rule("++++++.-."), //3
                    new Rule(".-.+++.-."), //4
                    new Rule("++++++-++"), //5
                    new Rule("-++++++++"), //6
                },

                //Column 3
                {
                    new Rule(".-.++-++."), //1
                    new Rule("++.++-++."), //2
                    new Rule("++.++-.-."), //3
                    new Rule(".-.++-.-."), //4
                    new Rule("++-+++++-"), //5
                    new Rule("-+-++++++"), //6
                },

                //Column 4
                {
                    new Rule(".-.-+-.+."), //1
                    new Rule(".+.-+-.+."), //2
                    new Rule(".+.-+-.-."), //3
                    new Rule(".-.-+-.-."), //4
                    new Rule("++++++-+-"), //5
                    new Rule("-+++++-++"), //6
                },

                //Column 5
                {
                    new Rule(".-.+++++-"), //1
                    new Rule("++-+++.-."), //2
                    new Rule(".++-++.+-"), //2
                    new Rule(".+--++.++"), //3
                    new Rule("++-+++-+-"), //5
                    new Rule("-+-+++++-"), //6
                },

                //Column 6
                {
                    new Rule(".-.+++-++"), //1
                    new Rule("-+++++.-."), //2
                    new Rule("++.++--+."), //3
                    new Rule("-+.++-++."), //4
                    new Rule("-+++++-+-"), //5
                    new Rule("-+-+++-++"), //6
                },

                //Column 7
                {
                    new Rule(".-.-++.+-"), //1
                    new Rule(".+--++.-."), //2
                    new Rule(".-.+++-+-"), //3
                    new Rule(".+--++.+-"), //4
                    new Rule("-+++++++-"), //5
                    new Rule("-+-+++-+-"), //6
                },

                //Column 8
                {
                    new Rule(".-.++--+."), //1
                    new Rule("-+.++-.-."), //2
                    new Rule("-+.++--+."), //3
                    new Rule("-+-+++.-."), //4
                    new Rule("++-+++-++"), //5
                    new Rule("---------"), //6
                }
        });

        public class Rule
        {
            public bool?[,] criteria = new bool?[3, 3];

            public bool CompareCode(string obj)
            {
                if (obj.Length != 9)
                {
                    throw new ArgumentException($"Invalid string inputted: \"{obj}\"");
                }

                bool equal = true;
                string code = GetCode();
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (criteria[i, j] != null)
                        {
                            if (obj[(i*3) + j] != code[(i*3) + j])
                            {
                                equal = false;
                                i = 3;
                                j = 3;
                            }
                        }
                        else
                        {
                            //Console.WriteLine($"Criteria <{i}, {j}> is null");
                        }
                    }
                }
                return equal;
            }

            public string GetCode()
            {
                string code = "";
                foreach (bool? b in criteria)
                {
                    if (b == null) code += ".";
                    if (b == true) code += "+";
                    if (b == false) code += "-";
                }
                return code;
            }
            
            public Rule(string code)
            {
                if (code.Length != 9)
                {
                    throw new FormatException();
                }

                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (code[i * 3 + j] == '.') criteria[i, j] = null;
                        if (code[i * 3 + j] == '+') criteria[i, j] = true;
                        if (code[i * 3 + j] == '-') criteria[i, j] = false;
                    }
                }
            }

            public Rule(bool? topLeft,
                bool? topCenter,
                bool? topRight,
                bool? left,
                bool? center,
                bool? right,
                bool? bottomLeft,
                bool? bottomCenter,
                bool? bottomRight)
            {
                criteria[0, 0] = topLeft;
                criteria[1, 0] = topCenter;
                criteria[2, 0] = topRight;
                criteria[0, 1] = left;
                criteria[1, 1] = center;
                criteria[2, 1] = right;
                criteria[0, 2] = bottomLeft;
                criteria[1, 2] = bottomCenter;
                criteria[2, 2] = bottomRight;
            }
        }
    }
}