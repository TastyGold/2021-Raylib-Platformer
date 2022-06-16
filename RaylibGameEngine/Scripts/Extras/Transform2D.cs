using System;
using System.Numerics;

namespace Components
{
    public class Transform2D
    {
        //Data
        private Vector2 _localPosition;

        //Properties
        public Transform2D Pivot { get; set; }
        public Vector2 Position
        {
            get
            {
                if (Pivot is null) return LocalPosition;
                else return Pivot.Position + LocalPosition;
            }
            set
            {
                if (Pivot is null) LocalPosition = value;
                else LocalPosition += value - Position;
            }
        }
        public Vector2 LocalPosition
        {
            get => _localPosition;
            set => _localPosition = value;
        }

        public Vector2 Size { get; set; }

        public delegate void PositionChangedDelegate();
        public event PositionChangedDelegate OnPositionChanged;
    }
}