using MathExtras;
using System.Numerics;
using Raylib_cs;
using PGui;

namespace Engine
{
    public class DebugCamera
    {
        public Camera2D cam;
        public bool locked = false;

        public const float speed = 600;
        public const float zoomPower = 0.25f;
        public static Vector2? mousePanOrigin = null;

        //Mouse position functions
        public Vector2 GetMouseWorldPosition()
        {
            Vector2 mousePos = Raylib.GetMousePosition();
            mousePos.Y = Screen.screenHeight - mousePos.Y;

            float screenTilesX = Screen.screenWidth / Screen.scalar / cam.zoom;
            float screenTilesY = Screen.screenHeight / Screen.scalar / cam.zoom;

            mousePos.X /= Screen.screenWidth / screenTilesX;
            mousePos.Y /= Screen.screenHeight / screenTilesY;

            Vector2 tileOffset = (cam.target - cam.offset * new Vector2(1, -1) / cam.zoom) / Screen.scalar * new Vector2(1, -1);
            mousePos += tileOffset;

            return mousePos;
        }
        public Vector2 GetMouseWorldPosition(Window window, out bool mouseOnWindow)
        {
            Vector2 mousePos = window.GetMousePosition(out mouseOnWindow);
            mousePos.Y = Screen.screenHeight - mousePos.Y;

            float screenTilesX = Screen.screenWidth / Screen.scalar / cam.zoom;
            float screenTilesY = Screen.screenHeight / Screen.scalar / cam.zoom;

            mousePos.X /= Screen.screenWidth / screenTilesX;
            mousePos.Y /= Screen.screenHeight / screenTilesY;

            Vector2 tileOffset = (cam.target - cam.offset * new Vector2(1, -1) / cam.zoom) / Screen.scalar * new Vector2(1, -1);
            mousePos += tileOffset;

            return mousePos;
        }

        //Camera movement methods
        public void HandleMovement(Vector2 mousePosition)
        {
            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_MIDDLE_BUTTON) && !locked || mousePanOrigin == null)
            {
                mousePanOrigin = mousePosition;
            }

            if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_MIDDLE_BUTTON) || locked)
            {
                mousePanOrigin = null;
            }

            if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_MIDDLE_BUTTON) && !locked)
            {
                cam.target -= (mousePosition - (Vector2)mousePanOrigin) * Vect.FlipY * Screen.scalar;
            }
            else
            {
                Vector2 cameraMoveVector = Input.GetWASDInput() * Vect.FlipY;
                float cameraZoomInput = Input.GetMouseScroll();
                float speedMod = Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT) ? 1.75f : 1;
                speedMod /= cam.zoom;

                if (!Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL))
                {
                    cam.target += cameraMoveVector * speed * speedMod * Raylib.GetFrameTime();
                }

                if (!locked) HandleZooming(mousePosition);
            }
        }
        private void HandleZooming(Vector2 mousePosition)
        {
            float scroll = Input.GetMouseScroll();

            if (scroll > 0)
            {
                //Zoom in
                cam.zoom *= 1 + zoomPower;
                cam.target = Vector2.Lerp(cam.target, Rendering.WorldVector(mousePosition), zoomPower * (1 - zoomPower));
            }
            if (scroll < 0)
            {
                //Zoom out
                cam.zoom /= 1 + zoomPower;
                cam.target = Vector2.Lerp(cam.target, Rendering.WorldVector(mousePosition), -zoomPower);
            }
        }

        //Intialisation
        public DebugCamera()
        {
            cam = new Camera2D
            {
                offset = new Vector2(Screen.screenWidth / 2, Screen.screenHeight / 2),
                target = new Vector2(Screen.screenWidth / 2, Screen.screenHeight / -2),
                zoom = 1f,
            };
        }
    }
}