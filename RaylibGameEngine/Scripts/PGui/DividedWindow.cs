using Raylib_cs;
using System;
using System.Numerics;

namespace PGui
{
    public class DividedWindow : Window
    {
        //Constants and Statics
        public const int dividerRegionWidth = 5;
        public const int minWindowResize = 150;
        public static bool performantWindowScaling = false;

        //Data
        public override Vector2 Position
        { 
            get => base.Position;
            set
            {
                A.Position += value - Position;
                B.Position += value - Position;

                base.Position = value;
            }
        }
        public override Vector2 Size
        {
            get => base.Size;
            set
            {
                if (this.mode == DividerMode.Horizontal)
                {
                    A.Size = new Vector2(A.Size.X, value.Y);
                    B.Size = new Vector2(B.Size.X + (value.X - Size.X), value.Y);
                }
                else
                {
                    A.Size = new Vector2(value.X, A.Size.Y);
                    B.Size = new Vector2(value.X, B.Size.Y + (value.Y - Size.Y));
                }
                
                base.Size = value;
            }
        }

        public Window A;
        public Window B;

        private int _dividerPos;
        public int DividerPosition
        {
            get => _dividerPos;
            set
            {
                int newValue = Math.Min((mode == DividerMode.Horizontal ? width : height) - minWindowResize, Math.Max(minWindowResize, value));
                if (mode == DividerMode.Horizontal)
                {
                    A.Size += new Vector2(newValue - _dividerPos, 0);
                    B.Size -= new Vector2(newValue - _dividerPos, 0);
                    B.Position += new Vector2(newValue - _dividerPos, 0);
                }
                else
                {
                    A.Size += new Vector2(0, newValue - _dividerPos);
                    B.Size -= new Vector2(0, newValue - _dividerPos);
                    B.Position += new Vector2(0, newValue - _dividerPos);
                }
                _dividerPos = newValue;
            }
        }
        public readonly DividerMode mode;
        public bool isDividerLocked = false;

        //Overrides
        public override void ReloadRenderTexture()
        {
            A.ReloadRenderTexture();
            B.ReloadRenderTexture();
        }
        public override void BeginDrawing()
        {
            throw new Exception("Divided window cannot begin drawing.");
        }
        public override void EndDrawing()
        {
            throw new Exception("Divided window cannot end drawing.");
        }
        public override void DrawToScreen()
        {
            A.DrawToScreen();
            B.DrawToScreen();
            DrawDivider();
        }
        public override void DrawOutline()
        {
            base.DrawOutline();
        }
        public override void Close()
        {
            A.Close();
            B.Close();
        }

        //Methods
        public Rectangle GetDividerRegion()
        {
            Rectangle r;
            if (mode == DividerMode.Horizontal)
            {
                r = new Rectangle(Position.X + DividerPosition - dividerRegionWidth, Position.Y, dividerRegionWidth * 2, Size.Y);
            }
            else
            {
                r = new Rectangle(Position.X, Position.Y + DividerPosition - dividerRegionWidth, Size.X, dividerRegionWidth * 2);
            }
            return r;
        }
        public void DrawDivider()
        {
            if (mode == DividerMode.Horizontal)
            {
                Raylib.DrawLineEx(Position + new Vector2(DividerPosition, 0), Position + new Vector2(DividerPosition, Size.Y), 2, UIColors.Gray(150));
            }
            else
            {
                Raylib.DrawLineEx(Position + new Vector2(0, DividerPosition), Position + new Vector2(Size.X, DividerPosition), 2, UIColors.Gray(150));
            }
        }
        public void DrawDividerOverlay()
        {
            Raylib.DrawRectangleRec(GetDividerRegion(), new Color(255, 0, 0, 50));
        }
        public int MouseDistanceToDivider()
        {
            return (mode == DividerMode.Horizontal ? (int)GetMousePosition().X : (int)GetMousePosition().Y) - DividerPosition;
        }
        public Window GetMousePriority()
        {
            int mousePos = MouseDistanceToDivider();
            if (isDividerLocked || Math.Abs(mousePos) > dividerRegionWidth)
            {
                return mousePos < 0 ? A : B;
            }
            else return null;
        }

        //Constructor
        public DividedWindow(int x, int y, int width, int height, int dividerPos, DividerMode mode)
        {
            this.X = x;
            this.Y = y;
            this.width = width;
            this.height = height;
            this._dividerPos = dividerPos;
            this.mode = mode;

            A = new Window(0, 0, mode == DividerMode.Horizontal ? dividerPos : width, mode == DividerMode.Vertical ? dividerPos : height, Color.RAYWHITE);
            B = mode == DividerMode.Horizontal ? new Window(x + dividerPos, y, width - dividerPos, height, Color.RAYWHITE) : new Window(x, y + dividerPos, width, height - dividerPos, Color.RAYWHITE);
        }
        public DividedWindow(Window house, Window a, Window b, int dividerPos, DividerMode mode) :
            this(house.X, house.Y, (int)house.Size.X, (int)house.Size.Y, a, b, dividerPos, mode) { }
        public DividedWindow(int x, int y, int width, int height, Window a, Window b, int dividerPos, DividerMode mode)
        {
            A = a;
            B = b;

            this._dividerPos = dividerPos;
            this.mode = mode;
            this.Position = new Vector2(x, y);
            this.Size = new Vector2(width, height);

            A.Position = new Vector2(x, y);

            if (mode == DividerMode.Horizontal)
            {
                A.Size = new Vector2(dividerPos, height);

                B.Position = new Vector2(x + dividerPos, y);
                B.Size = new Vector2(width - dividerPos, height);
            }
            else
            {
                A.Size = new Vector2(width, dividerPos);

                B.Position = new Vector2(x, y + dividerPos);
                B.Size = new Vector2(width, height - dividerPos);
            }
        }

        public enum DividerMode
        { 
            Horizontal,
            Vertical
        }
    }
}