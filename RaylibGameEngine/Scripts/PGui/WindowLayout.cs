using System;
using System.Collections;
using System.Numerics;
using Raylib_cs;
using static PGui.InputHelper;

namespace PGui
{
    public class WindowLayout
    {
        //Data
        private int width;
        private int height;

        private Window windows;
        public MouseHandler mouseHandler = new MouseHandler();

        //Methods
        public void DrawAll()
        {
            windows.DrawToScreen();
        }
        public void RefreshMousePriority()
        {
            if (!mouseHandler.isPriorityLocked && !(Held_MB || Released_MB))
                mouseHandler.CalculateMousePriority(windows);
        }
        public void InitialiseRenderTextures()
        {
            windows.ReloadRenderTexture();
        }

        private int tempMouseOffset;
        public void HandleResizing()
        {
            if (mouseHandler.priorityMode == MousePriority.Divider && Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON) && !mouseHandler.isPriorityLocked)
            {
                mouseHandler.isPriorityLocked = true;
                mouseHandler.SetMouseButtonHeld(true);
                tempMouseOffset =
                    mouseHandler.DivPrio.mode == DividedWindow.DividerMode.Horizontal ?
                    (int)(mouseHandler.mouseDownPosition.X - mouseHandler.DivPrio.DividerPosition) :
                    (int)(mouseHandler.mouseDownPosition.Y - mouseHandler.DivPrio.DividerPosition);
            }
            else if (mouseHandler.priorityMode == MousePriority.Divider && Raylib.IsMouseButtonDown(MouseButton.MOUSE_LEFT_BUTTON) && mouseHandler.isPriorityLocked && mouseHandler.isMouseHeld)
            {
                mouseHandler.DivPrio.DividerPosition = 
                    mouseHandler.DivPrio.mode == DividedWindow.DividerMode.Horizontal ?
                    (int)(mouseHandler.mouseDownPosition.X + mouseHandler.MouseHeldDeltaPos.X) - tempMouseOffset:
                    (int)(mouseHandler.mouseDownPosition.Y + mouseHandler.MouseHeldDeltaPos.Y) - tempMouseOffset;

                if (!DividedWindow.performantWindowScaling && mouseHandler.MouseDeltaPosition != Vector2.Zero) mouseHandler.DivPrio.ReloadRenderTexture();
            }
            if (mouseHandler.priorityMode == MousePriority.Divider && Raylib.IsMouseButtonUp(MouseButton.MOUSE_LEFT_BUTTON) && mouseHandler.isPriorityLocked)
            {
                mouseHandler.isPriorityLocked = false;
                mouseHandler.SetMouseButtonHeld(false);

                if (DividedWindow.performantWindowScaling) mouseHandler.DivPrio.ReloadRenderTexture();
            }
        }

        //Constructor
        public WindowLayout()
        {
            this.width = Engine.Screen.screenWidth;
            this.height = Engine.Screen.screenHeight;
            this.mouseHandler = new MouseHandler();
        }
        public WindowLayout(Window windows) : base()
        {
            this.windows = windows;
        }
    }

    public class MouseHandler
    {
        //Data
        public MousePriority priorityMode = MousePriority.None;
        public bool isPriorityLocked = false;

        public bool isMouseHeld = false;
        public Vector2 mouseDownPosition;
        public Vector2 mouseCurrentPosition;
        private Vector2 lastMousePosition = Vector2.Zero;
        public Vector2 MouseDeltaPosition => mouseCurrentPosition - lastMousePosition;
        public Vector2 MouseHeldDeltaPos => mouseCurrentPosition - mouseDownPosition;

        //Properties
        private Window _priority = null;
        public Window Priority
        {
            get => priorityMode == MousePriority.Window ? _priority : null;
            private set
            {
                if (_priority != null) _priority.prioritised = false;
                if (value != null) value.prioritised = true;
                _priority = value;
            }
        }

        private DividedWindow _divPrio = null;
        public DividedWindow DivPrio
        {
            get => priorityMode == MousePriority.Divider ? _divPrio : null;
            private set => _divPrio = value;
        }

        //Methods
        public void RecordMousePosition()
        {
            lastMousePosition = mouseCurrentPosition;
            mouseCurrentPosition = Raylib.GetMousePosition();
        }
        public void SetMouseButtonHeld(bool state)
        {
            if (state) mouseDownPosition = mouseCurrentPosition;
            isMouseHeld = state;
        }
        public void CalculateMousePriority(Window window)
        {
            for (int terminate = 0; terminate < 100; terminate++)
            {
                if (window is DividedWindow divWindow)
                {
                    window = divWindow.GetMousePriority();
                    if (window == null)
                    {
                        priorityMode = MousePriority.Divider;
                        DivPrio = divWindow;
                        return;
                    }
                }
                else
                {
                    priorityMode = MousePriority.Window;
                    Priority = window;
                    return;
                }
            }
            throw new Exception("Mouse priority error");
        }
        public void UpdateMouseSprite()
        {
            if (priorityMode == MousePriority.Divider)
            {
                Raylib.SetMouseCursor(DivPrio.mode == DividedWindow.DividerMode.Horizontal ? MouseCursor.MOUSE_CURSOR_RESIZE_EW : MouseCursor.MOUSE_CURSOR_RESIZE_NS);
            }
            else
            {
                Raylib.SetMouseCursor(MouseCursor.MOUSE_CURSOR_DEFAULT);
            }
        }
        public void HighlightMousePriority(MousePriority priority = MousePriority.None)
        {
            if (priorityMode == MousePriority.Window && (priority == MousePriority.None || priority == MousePriority.Window))
            {
                Priority.DrawOverlay();
            }
            else if (priorityMode == MousePriority.Divider && (priority == MousePriority.None || priority == MousePriority.Divider))
            {
                DivPrio.DrawDividerOverlay();
            }
        }
    }

    public enum MousePriority
    {
        None,
        Window,
        Divider,
    }
}