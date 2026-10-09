using System;
using UnityEngine;

namespace Commar.CubicLand
{
    public static class DirectionUtils
    {
        private static readonly int _directionLength = Enum.GetValues(typeof(Direction)).Length;
        private static readonly int _direction3DLength = Enum.GetValues(typeof(Direction3D)).Length;
        private static readonly Vector3[] _directions = new Vector3[] 
        { 
            Vector3.right, 
            Vector3.left, 
            Vector3.up, 
            Vector3.down, 
            Vector3.forward, 
            Vector3.back 
        };

        public static int DirectionLength => _directionLength;
        public static int Direction3DLength => _direction3DLength;

        public static Vector3Int GetDirection3DValue(Direction3D direction)
        {
            switch (direction)
            {
                case Direction3D.Right:
                    return Vector3Int.right;
                case Direction3D.Left:
                    return Vector3Int.left;
                case Direction3D.Up:
                    return Vector3Int.up;
                case Direction3D.Down:
                    return Vector3Int.down;
                case Direction3D.Front:
                default:
                    return Vector3Int.forward;
                case Direction3D.Back:
                    return Vector3Int.back;
            }
        }

        public static Direction3D ConvertDirection2DTo3D(Direction direction)
        {
            switch (direction)
            {
                default:
                case Direction.Right:
                    return Direction3D.Right;
                case Direction.Left:
                    return Direction3D.Left;
                case Direction.Front:
                    return Direction3D.Front;
                case Direction.Back:
                    return Direction3D.Back;
            }
        }

        public static Direction ConvertDirection3DTo2D(Direction3D direction)
        {
            switch (direction)
            {
                default:
                case Direction3D.Right:
                    return Direction.Right;
                case Direction3D.Left:
                    return Direction.Left;
                case Direction3D.Front:
                    return Direction.Front;
                case Direction3D.Back:
                    return Direction.Back;
            }
        }

        public static Direction3D ConvertDirection(Vector3[] directions, Vector3 direction)
        {
            Utils.GetClosestAxisVector(directions, direction, out int index);
            return (Direction3D)index;
        }

        public static Direction3D ConvertDirection(Vector3 direction)
        {
            return ConvertDirection(_directions, direction);
        }
    }
}