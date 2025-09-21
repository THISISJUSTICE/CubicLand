using UnityEngine;

namespace CustomTIJI.CubicLand
{
    [RequireComponent(typeof(SkillGolemCube))]
    public class SkillGolemCore : CubeCore<SkillGolemCube>
    {
        #region Variables
        protected GolemController _owner;

        private Vector3 _moveVelocity;

        protected override Vector3 MoveVelocity => _moveVelocity;
        #endregion

        #region Unity Functions
        protected override void Awake()
        {
            base.Awake();
            _golemCube = GetComponent<SkillGolemCube>();
        }

        private void FixedUpdate()
        {
            _moveVelocity = _rb.linearVelocity;
        }

        protected override void OnCollisionEnter(Collision collision)
        {
            _rb.linearVelocity = _moveVelocity;

            if (collision == null)
                return;

            if (collision.gameObject == _owner.gameObject)
                return;

            if (collision.gameObject.CompareTag("Map"))
            {
                ActOnChildCollsion(collision, (child) =>
                {
                    this.WaitFrameAct(1, () => child.OnDamaged(_rb.mass, collision.impulse));
                });
                return;
            }

            OnCoreCollision(collision);
        }
        #endregion

        #region Public Functions
        public void SetInit(GolemInfo golemInfo, GolemController owner)
        {
            SetupChilds();
            _owner = owner;
            SetGolemInfo(golemInfo);

            _moveVelocity = Vector3.zero;

            _rb.isKinematic = false;
            _rb.useGravity = false;
            _rb.constraints = RigidbodyConstraints.FreezeAll;
            SetGolemMass();
        }
        #endregion

        #region Utils
        #endregion
    }
}