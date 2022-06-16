using Raylib_cs;
using System.Collections;
using System.Collections.Generic;

namespace Engine
{
    public static class DebugText
    {
        //Config
        private static Color defaultTextColor = Color.LIGHTGRAY;
        private static int bgWidth = 200;
        private static int yOffset = 14;

        //Runtime
        private static List<WriteTicket> lateTickets = new List<WriteTicket>();
        private static int lines;
        private static int maxLines;

        //Methods
        public static void Clear()
        {
            lines = 0;
            lateTickets.Clear();
        }
        public static void Write(string prefix, object data)
        {
            Write(prefix, data, defaultTextColor);
        }
        public static void Write(string prefix, object data, Color color)
        {
            Raylib.DrawText($"{prefix}: {data.ToString()}", 10, yOffset + (20*lines), 20, color);
            lines++;
            if (maxLines < lines) maxLines = lines;
        }
        public static void WriteLate(string prefix, object data)
        {
            WriteLate(prefix, data, defaultTextColor);
        }
        public static void WriteLate(string prefix, object data, Color color)
        {
            lateTickets.Add(new WriteTicket(prefix, data, color));
        }
        public static void WriteTickets()
        {
            for (int i = 0; i < lateTickets.Count; i++)
            {
                Write(lateTickets[i]._prefix, lateTickets[i]._data, lateTickets[i]._color);
            }
        }
        public static void WriteFPS()
        {
            Raylib.DrawFPS(10, yOffset + (20 * lines));
            lines++;
        }
        public static void WriteTitle(string title)
        {
            Raylib.DrawText(title, 10, 12 + (20 * lines), 20, defaultTextColor);
            lines++;
            if (maxLines < lines) maxLines = lines;
        }
        public static void DrawBackground()
        {
            Raylib.DrawRectangle(0, 0, bgWidth, (maxLines + 1) * 20, new Color(75, 75, 75, 175));
        }

        //Ticket containing Write() data that can be stored for later use
        private struct WriteTicket
        {
            public string _prefix;
            public object _data;
            public Color _color;

            public WriteTicket(string prefix, object data, Color color)
            {
                _prefix = prefix;
                _data = data;
                _color = color;
            }
        }
    }
}