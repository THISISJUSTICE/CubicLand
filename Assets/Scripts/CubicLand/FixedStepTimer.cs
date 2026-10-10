using UnityEngine;

namespace Commar.CubicLand
{
    public struct FixedStepTimer
    {
        private readonly float _duration;
        private float _time;

        public static YieldInstruction YieldWait => YieldCache.WaitForFixedUpdate;
        public float Progress => _duration <= 0f ? 1f : Mathf.Clamp01(_time / _duration);
        public bool IsCompleted { get; private set; }

        public FixedStepTimer(float duration)
        {
            _duration = Mathf.Max(0f, duration);
            _time = 0f;
            IsCompleted = false;
        }

        public YieldInstruction Advance()
        {
            if (_time >= _duration)
                IsCompleted = true;
            else
                _time = Mathf.Min(_time + Time.fixedDeltaTime, _duration);

            return YieldWait;
        }
    }
}