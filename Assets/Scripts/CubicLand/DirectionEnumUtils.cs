using System;
using UnityEngine;

namespace Commar.CubicLand
{
    public static class DirectionEnumUtils
    {
        private static readonly int _directionLength = Enum.GetValues(typeof(Enums.Direction)).Length;
        private static readonly int _direction3DLength = Enum.GetValues(typeof(Enums.Direction3D)).Length;
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

        public static Vector3Int GetDirection3DValue(Enums.Direction3D direction)
        {
            switch (direction)
            {
                case Enums.Direction3D.Right:
                    return Vector3Int.right;
                case Enums.Direction3D.Left:
                    return Vector3Int.left;
                case Enums.Direction3D.Up:
                    return Vector3Int.up;
                case Enums.Direction3D.Down:
                    return Vector3Int.down;
                case Enums.Direction3D.Front:
                default:
                    return Vector3Int.forward;
                case Enums.Direction3D.Back:
                    return Vector3Int.back;
            }
        }

        public static Enums.Direction3D ConvertDirection2DTo3D(Enums.Direction direction)
        {
            switch (direction)
            {
                default:
                case Enums.Direction.Right:
                    return Enums.Direction3D.Right;
                case Enums.Direction.Left:
                    return Enums.Direction3D.Left;
                case Enums.Direction.Front:
                    return Enums.Direction3D.Front;
                case Enums.Direction.Back:
                    return Enums.Direction3D.Back;
            }
        }

        public static Enums.Direction ConvertDirection3DTo2D(Enums.Direction3D direction)
        {
            switch (direction)
            {
                default:
                case Enums.Direction3D.Right:
                    return Enums.Direction.Right;
                case Enums.Direction3D.Left:
                    return Enums.Direction.Left;
                case Enums.Direction3D.Front:
                    return Enums.Direction.Front;
                case Enums.Direction3D.Back:
                    return Enums.Direction.Back;
            }
        }

        public static Enums.Direction3D ConvertDirection(Vector3[] directions, Vector3 direction)
        {
            Utils.GetClosestAxisVector(directions, direction, out int index);
            return (Enums.Direction3D)index;
        }

        public static Enums.Direction3D ConvertDirection(Vector3 direction)
        {
            return ConvertDirection(_directions, direction);
        }
    }
}