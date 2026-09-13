using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace Commar.CubicLand.Compositions
{
    public class UnityLoopHandler : MonoBehaviour
    {
        private IReadOnlyList<IUpdatable> _updatables;
        private IReadOnlyList<IFixedUpdatable> _fixedUpdatables;

        [Inject]
        public void Initialize(IReadOnlyList<IUpdatable> updatables, IReadOnlyList<IFixedUpdatable> fixedUpdatables)
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