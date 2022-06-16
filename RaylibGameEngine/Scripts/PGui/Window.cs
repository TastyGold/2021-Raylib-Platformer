using System;
using System.Numerics;
using Raylib_cs;
using static PGui.InputHelper;

namespace PGui
{
    public class Window
    {
        //Data
        public int X { get; set; }
        public int Y { get; set; }
        public virtual Vector2 Position
        {
            get => new Vector2(X, Y);
            set
            {
                X = (int)value.X;
                Y = (int)value.Y;
            }
        }
        protected int width;
        protected int height;
        protected int minWidth;
        protected int minHeight;
        public virtual Vector2 MinSize
        {
            get => new Vector2(minWidth, minHeight);
            set
            {
                minWidth = (int)value.X;
                minHeight = (int)value.Y;
            }
        }
        public virtual Vector2 Size
        {
            get => new Vector2(width, height);
            set
            {
                width = (int)value.X;
                height = (int)value.Y;
            }
        }
        public virtual Vector2 TextureSize
        {
            get => new Vector2(renderTexture.texture.width, renderTexture.texture.height);
            set
            {
                renderTexture.texture.width = (int)value.X;
                renderTexture.texture.height = (int)value.Y;
            }
        }

        //Rendering
        private bool isRenderTextureLoaded = false;
        private RenderTexture2D renderTexture;
        private Color bgColor = Color.RAYWHITE;
        private Color outlineColor = UIColors.Gray(130);
        private readonly bool drawOutline = false;

        //Mouse
        public bool prioritised;
        public Vector2 GetMousePosition()
        {
            return Raylib.GetMousePosition() - new Vector2(X, Y - ScrollAmount);
        }
        public Vector2 GetMousePosition(out bool isMouseOver)
        {
            Vector2 mousePos = GetMousePosition();
            isMouseOver = mousePos.X < 0 || mousePos.Y < 0 || mousePos.X > width || mousePos.Y > height;
            return mousePos;
        }

        //Scrolling
        public bool enableScrolling = false;
        private float _scroll;
        private float scrollSpeed = 20;
        public float ScrollAmount
        {
            get => _scroll;
            set
            {
                if (value < 0) _scroll = 0;
                else _scroll = value;
            }
        }
        public void HandleScrolling()
        {
            if (prioritised)
            {
                float scrollInput = Raylib.GetMouseWheelMove();
                ScrollAmount -= scrollInput * scrollSpeed;
            }
        }

        //Methods
        public virtual void BeginDrawing()
        {
            Raylib.BeginTextureMode(renderTexture);
            Raylib.ClearBackground(bgColor);
        }
        public virtual void EndDrawing()
        {
            Raylib.EndTextureMode();
        }
        public virtual void DrawToScreen()
        {
            Raylib.DrawTextureRec(renderTexture.texture, new Rectangle(0, 0, width, -height), new Vector2(X, Y), Color.WHITE);
            if (drawOutline) DrawOutline();
        }
        public virtual void DrawOutline()
        {
            Raylib.DrawRectangleLinesEx(new Rectangle(X, Y, width, height), 1, outlineColor);
        }
        public virtual void Close()
        {
            Raylib.UnloadRenderTexture(renderTexture);
            isRenderTextureLoaded = false;
        }
        public virtual void DrawOverlay()
        {
            Raylib.DrawRectangle(X, Y, width, height, new Color(255, 0, 0, 50));
        }
        public virtual void ReloadRenderTexture()
        {
            Raylib.UnloadRenderTexture(renderTexture);
            renderTexture = Raylib.LoadRenderTexture(width, height);
            isRenderTextureLoaded = true;
        }

        //Constructor
        public Window()
        {
            X = 0;
            Y = 0;
            width = 0;
            height = 0;
        }
        public Window(Color bgColor, bool enableScrolling = false) : this()
        {
            this.bgColor = bgColor;
            this.enableScrolling = enableScrolling;
        }
        public Window(int x, int y, int width, int height, Color bgColor, bool drawOutline = false, bool enableScrolling = false, bool autoInitTexture = false)
        {
            this.X = x;
            this.Y = y;
            this.width = width;
            this.height = height;
            this.bgColor = bgColor;
            this.drawOutline = drawOutline;
            this.enableScrolling = enableScrolling;
            this.isRenderTextureLoaded = autoInitTexture;
            if (autoInitTexture) renderTexture = Raylib.LoadRenderTexture(width, height);
        }
    }

    public interface IGuiElement
    {
        public void Draw();
        public UIRect Rect { get; set; }
        public Window ParentWindow { get; set; }
    }

    public enum UIHandle
    {
        topLeft,
        topMiddle,
        topRight,
        middleLeft,
        center,
        middleRight,
        bottomLeft,
        bottomMiddle,
        bottomRight,
    }

    public struct UIRect
    {
        private int x;
        private int y;
        private int width;
        private int height;

        public Rectangle RawRect
        {
            get => new Rectangle(x, y, width, height);
            private set
            {
                x = (int)value.x;
                y = (int)value.y;
                width = (int)value.width;
                height = (int)value.height;
            }
        }

        private UIHandle _handle;
        public UIHandle Handle
        {
            get => _handle;
            set
            {
                Vector2 originalOffset = GetHandleLocalOffset(_handle) + GetHandleWindowOffset(_handle);
                Vector2 newOffset = GetHandleLocalOffset(value) + GetHandleWindowOffset(value);

                Vector2 transformVector = newOffset - originalOffset;

                x += (int)transformVector.X;
                y += (int)transformVector.Y;

                _handle = value;
            }
        }

        private Vector2 windowSize;
        public void SetWindowSize(Vector2 size)
        {
            width = (int)size.X;
            height = (int)size.Y;
        }

        public Rectangle GetWindowRect(float scroll = 0)
        {
            Vector2 offset = GetHandleWindowOffset(_handle) + GetHandleLocalOffset(_handle);
            return new Rectangle(offset.X, offset.Y - scroll, width, height);
        }

        public Vector2 GetHandleLocalOffset(UIHandle handle)
        {
            return handle switch
            {
                UIHandle.topLeft => new Vector2(x, y),
                UIHandle.topMiddle => new Vector2(x - (width / 2), y),
                UIHandle.topRight => new Vector2(x - width, y),
                UIHandle.middleLeft => new Vector2(x, y - (height / 2)),
                UIHandle.center => new Vector2(x - (width / 2), y - (height / 2)),
                UIHandle.middleRight => new Vector2(x - width, y - (height / 2)),
                UIHandle.bottomLeft => new Vector2(x, y - height),
                UIHandle.bottomMiddle => new Vector2(x - (width / 2), y - height),
                UIHandle.bottomRight => new Vector2(x - width, y - height),
                _ => throw new NullReferenceException("Handle cannot be null"),
            };
        }
        public Vector2 GetHandleWindowOffset(UIHandle handle)
        {
            return handle switch
            {
                UIHandle.topLeft => new Vector2(0, 0),
                UIHandle.topMiddle => new Vector2(windowSize.X / 2, 0),
                UIHandle.topRight => new Vector2(windowSize.X, 0),
                UIHandle.middleLeft => new Vector2(0, windowSize.Y / 2),
                UIHandle.center => new Vector2(windowSize.X / 2, windowSize.Y / 2),
                UIHandle.middleRight => new Vector2(windowSize.X, windowSize.Y / 2),
                UIHandle.bottomLeft => new Vector2(0, windowSize.Y),
                UIHandle.bottomMiddle => new Vector2(windowSize.X / 2, windowSize.Y),
                UIHandle.bottomRight => new Vector2(windowSize.X, windowSize.Y),
                _ => throw new NullReferenceException("Handle cannot be null"),
            };
        }

        public bool IsMouseOver(Vector2 mousePos)
        {
            return !(mousePos.X > x + width || mousePos.X < x || mousePos.Y > y + height || mousePos.Y < y);
        }

        public UIRect(Rectangle windowRect, Vector2 windowSize, UIHandle handle)
        {
            x = (int)windowRect.x;
            y = (int)windowRect.y;
            width = (int)windowRect.width;
            height = (int)windowRect.height;
            this.windowSize = windowSize;
            _handle = handle;
        }
        public UIRect(string text, int fontSize, int textPadding, Vector2 position, Vector2 windowSize, UIHandle handle)
        {
            int textLength = Raylib.MeasureText(text, fontSize);
            Rectangle rect = new Rectangle(position.X, position.Y, textLength + (textPadding * 2), fontSize + (textPadding * 2));

            x = (int)rect.x;
            y = (int)rect.y;
            width = (int)rect.width;
            height = (int)rect.height;
            this.windowSize = windowSize;
            _handle = handle;
        }
    }

    public struct UIElementColors
    {
        public Color outline;
        public Color inactive;
        public Color mouseOver;
        public Color pressed;
    }

    public static class UIColors
    {
        public static Color Gray(int value) => new Color(value, value, value, 255);

        public static UIElementColors buttonColors = new UIElementColors()
        {
            outline = Gray(100),
            inactive = Gray(230),
            mouseOver = Gray(220),
            pressed = Gray(210),
        };

    }

    public class UIButton : IGuiElement
    {
        //Data
        public readonly string text = "Button";
        public readonly int fontSize = 20;
        public readonly UIElementColors colors = UIColors.buttonColors;

        public UIRect Rect { get; set; }
        public Window ParentWindow { get; set; }

        //Methods
        public void Update()
        {
            if (IsMouseOver() && Clicked_LMB)
            {
                OnButtonPressed.Invoke();
            }
        }
        public void Draw()
        {
            Rect.SetWindowSize(ParentWindow.Size);
            Rectangle r = Rect.GetWindowRect(ParentWindow.ScrollAmount);
            Raylib.DrawRectangleRec(r, GetState());
            DrawButtonText();
            Raylib.DrawRectangleLinesEx(r, 1, colors.outline);
        }
        private void DrawButtonText()
        {
            Rectangle r = Rect.GetWindowRect();
            int textOffset = Raylib.MeasureText(text, fontSize);
            Vector2 textRec = new Vector2(r.x + ((r.width - textOffset) / 2), r.y - ParentWindow.ScrollAmount + ((r.height - fontSize) / 2));
            Raylib.DrawText(text, (int)textRec.X, (int)textRec.Y, fontSize, colors.outline);
        }
        private Color GetState()
        {
            return ParentWindow.prioritised ?
                IsMouseOver() ?
                    Raylib.IsMouseButtonDown(MouseButton.MOUSE_LEFT_BUTTON) ? colors.pressed : colors.mouseOver :
                    colors.inactive : 
                colors.inactive;
        }
        private bool IsMouseOver()
        {
            if (!ParentWindow.prioritised) return false;
            Vector2 mousePos = ParentWindow.GetMousePosition(out _);
            return Rect.IsMouseOver(mousePos);
        }

        public delegate void ButtonPressedHandler();
        public event ButtonPressedHandler OnButtonPressed;

        //Constructor
        public UIButton(string buttonText, Rectangle rect, Window parent, UIHandle handle = UIHandle.topLeft)
        {
            ParentWindow = parent;
            text = buttonText;
            Rect = new UIRect(rect, parent.Size, handle);
        }
        public UIButton(string buttonText, int textSize, int textPadding, Vector2 position, Window parent, UIHandle handle = UIHandle.topLeft)
        {
            ParentWindow = parent;
            text = buttonText;
            fontSize = textSize;
            Rect = new UIRect(buttonText, textSize, textPadding, position, parent.Size, handle);
        }
    }
    public class UICheckbox : IGuiElement
    {
        //Data
        public UIElementColors colors = UIColors.buttonColors;

        public UIRect Rect { get; set; }
        public Window ParentWindow { get; set; }

        //Methods
        public void Draw()
        {
            Raylib.DrawRectangleRec(Rect.GetWindowRect(ParentWindow.ScrollAmount), GetState());
            Raylib.DrawRectangleLinesEx(Rect.GetWindowRect(ParentWindow.ScrollAmount), 1, colors.outline);
        }
        private Color GetState()
        {
            return ParentWindow.prioritised ?
                IsMouseOver() ?
                    Raylib.IsMouseButtonDown(MouseButton.MOUSE_LEFT_BUTTON) ? colors.pressed : colors.mouseOver :
                    colors.inactive :
                colors.inactive;
        }
        private bool IsMouseOver()
        {
            if (!ParentWindow.prioritised) return false;
            Vector2 mousePos = ParentWindow.GetMousePosition(out _);
            return Rect.IsMouseOver(mousePos);
        }

        //Constructor
        public UICheckbox(Rectangle rect, Window parent, UIHandle handle)
        {
            Rect = new UIRect(rect, parent.Size, handle);
            ParentWindow = parent;
        }
    }
    public class UILabel : IGuiElement
    {
        //Data
        public readonly string text = "Label";
        public readonly int fontSize = 30;
        public readonly Color textColor = UIColors.Gray(120);

        public UIRect Rect { get; set; }
        public Window ParentWindow { get; set; }

        //Methods
        public void Draw()
        {
            Rectangle r = Rect.GetWindowRect();
            int textOffset = Raylib.MeasureText(text, fontSize);
            Vector2 textRec = new Vector2(r.x + ((r.width - textOffset) / 2), r.y - ParentWindow.ScrollAmount + ((r.height - fontSize) / 2));
            Raylib.DrawText(text, (int)textRec.X, (int)textRec.Y, fontSize, textColor);
        }

        //Constructor
        public UILabel(string labelText, int textSize, int textPadding, Vector2 position, Window parent, UIHandle handle = UIHandle.topLeft)
        {
            text = labelText;
            fontSize = textSize;
            ParentWindow = parent;
            Rect = new UIRect(labelText, textSize, textPadding, position, parent.Size, handle);
        }
    }
}