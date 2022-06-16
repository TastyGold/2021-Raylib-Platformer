using System.Numerics;
using MathExtras;
using Raylib_cs;

namespace Engine
{
    public class CameraController
    {
        public Camera2D _camera;
        public Camera2D Camera
        {
            get
            {
                _camera.zoom = isWideView ? Zoom / wideViewScale : Zoom;
                return _camera;
            }
        }
        public bool isWideView = false;
        private const int wideViewScale = 2;

        private Vector2 _target;
        public Vector2 Target
        {
            get => _target;
            set
            {
                _camera.target = value * Screen.scalar * Vect.FlipY;
                _target = value;
            }
        }

        private float _zoom;
        public float Zoom 
        {
            get => _zoom;
            set
            {
                _size = (new Vector2(Screen.screenWidth, Screen.screenHeight) / Screen.scalar) * (1 / value);
                _zoom = value;
            }
        }

        private Vector2 _size;
        public Vector2 Size => _size;

        public Vectex GetWorldBounds()
        {
            return new Vectex
            {
                min = Target - (Size * 0.5f),
                max = Target + (Size * 0.5f)
            };
        }

        public void DrawCameraBounds()
        {
            Vectex bounds = GetWorldBounds();
            Raylib.DrawRectangleLinesEx(Rendering.GetScreenRect(bounds.min, bounds.Size), isWideView ? wideViewScale : 1, Color.WHITE);
            Rendering.CountDrawCallSimple();
        }

        public CameraController()
        {
            _camera = new Camera2D()
            {
                offset = new Vector2(Screen.screenWidth / 2, Screen.screenHeight / 2),
                zoom = 1,
                rotation = 0,
            };
        }
    }
}