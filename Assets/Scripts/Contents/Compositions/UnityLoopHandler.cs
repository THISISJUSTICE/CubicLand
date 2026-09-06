using UnityEngine;
using VContainer;

namespace Commar.CubicLand.Compositions
{
    public class UnityLoopHandler : MonoBehaviour
    {
        private IUpdatable[] _updatables;
        private IFixedUpdatable[] _fixedUpdatables;

        [Inject]
        public void Initialize(IUpdatable[] updatables, IFixedUpdatable[] fixedUpdatables)
        {
            _updatables = updatables;
            _fixedUpdatables = fixedUpdatables;
        }

        private void Update()
        {
            foreach (IUpdatable updatable in _updatables)
                updatable?.Update();
        }

        private void FixedUpdate()
        {
            foreach (IFixedUpdatable fixedUpdatable in _fixedUpdatables)
                fixedUpdatable?.FixedUpdate();
        }
    }
}