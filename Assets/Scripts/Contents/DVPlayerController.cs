using UnityEngine;
using System.Collections;

public class DVPlayerController : MonoBehaviour
{
    #region Variables
    protected Rigidbody _rb;
    protected bool _moving = false;

    protected int AnimationFrame { 
        get 
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
            return 30;
#else
            return 15;
#endif
        } 
    }
#endregion

    #region Unity Functions
    protected void Awake()
    {
        //_rb = GetComponent<Rigidbody>();
    }

    protected void Start()
    {
        DVKeyboardManager.Instance.SetKeyDown(KeyCode.W, () => { MoveGolem(DVEnums.Direction.FRONT, 1f); });
    }
    #endregion

    #region Controller
    protected void MoveGolem(DVEnums.Direction direction, float time) {
        if (_moving)
            return;

        StartCoroutine(OneCubeMoveCor(GetDirection(transform, direction), time));
    }

    protected void MoveGolemWithJump(DVEnums.Direction direction, float time, float jumpHeight)
    { 

    }
    #endregion

    #region Utils
    protected Vector3 GetDirection(Transform tf, DVEnums.Direction direction) {
        switch (direction) {
            case DVEnums.Direction.FRONT:
                return tf.forward;
            case DVEnums.Direction.BACK:
                return -tf.forward;
            case DVEnums.Direction.LEFT:
                return -tf.right;
            case DVEnums.Direction.RIGHT:
                return tf.right;
        }

        return Vector3.zero;
    }
    #endregion

    #region Coroutines
    protected IEnumerator OneCubeMoveCor(Vector3 dir, float time) {
        _moving = true;

        dir = dir.normalized * DVConfigs.CUBE_BASE_LENGHT;
        Vector3 addMove = dir / (float)AnimationFrame;
        float addTime = time / (float)AnimationFrame;

        for (int i = 0; i < AnimationFrame; i++) {
            transform.position += addMove;
            yield return DVHelper.Instance.YieldCache.GetWaitForSeconds(addTime);
        }

        _moving = false;
    }
    #endregion
}
