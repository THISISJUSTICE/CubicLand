using Commar.CubicLand.DataStorage;
using System;
using UnityEngine;

namespace Commar.CubicLand.Player
{
    public sealed class PlayerKeyboardInputSource : IPlayerInputSource
    {
        [Serializable]
        public struct PlayerKeyboardBindings
        {
            public KeyCode moveFront;
            public KeyCode moveRight;
            public KeyCode moveLeft;
            public KeyCode moveBack;

            public KeyCode rotateRight;
            public KeyCode rotateLeft;
            public KeyCode jump;

            public static PlayerKeyboardBindings Default => new()
            {
                moveFront = KeyCode.W,
                moveRight = KeyCode.D,
                moveLeft = KeyCode.A,
                moveBack = KeyCode.S,
                rotateRight = KeyCode.E,
                rotateLeft = KeyCode.Q,
                jump = KeyCode.Space
            };
        }

        private const string STORAGE_KEY = "PlayerKeyboardBindings";

        private readonly IDataStorage _dataStorage;
        private PlayerKeyboardBindings _bindings;

        public PlayerKeyboardBindings Bindings => _bindings;

        public PlayerKeyboardInputSource(IDataStorage dataStorage)
        {
            _dataStorage = dataStorage;

            if (!_dataStorage.TryLoad(STORAGE_KEY, out _bindings))
                _bindings = PlayerKeyboardBindings.Default;
        }

        public void ApplyKeyCode(PlayerKeyboardBindings bindings)
        {
            _bindings = bindings;
            _dataStorage.Save(STORAGE_KEY, _bindings);
        }

        public bool GetInput(PlayerInputType input)
        {
            return Input.GetKey(GetKeyCode(input));
        }

        public bool GetInputDown(PlayerInputType input)
        {
            return Input.GetKeyDown(GetKeyCode(input));
        }

        public bool GetInputUp(PlayerInputType input)
        {
            return Input.GetKeyUp(GetKeyCode(input));
        }

        private KeyCode GetKeyCode(PlayerInputType input)
        {
            switch (input)
            {
                case PlayerInputType.MoveFront:
                    return _bindings.moveFront;
                case PlayerInputType.MoveRight:
                    return _bindings.moveRight;
                case PlayerInputType.MoveLeft:
                    return _bindings.moveLeft;
                case PlayerInputType.MoveBack:
                    return _bindings.moveBack;
                case PlayerInputType.RotateRight:
                    return _bindings.rotateRight;
                case PlayerInputType.RotateLeft:
                    return _bindings.rotateLeft;
                case PlayerInputType.Jump:
                    return _bindings.jump;
            }

            return KeyCode.None;
        }
    }
}