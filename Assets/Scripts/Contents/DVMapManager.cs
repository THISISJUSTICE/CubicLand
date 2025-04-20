using UnityEngine;
using System.Collections;

public class DVMapManager : MonoBehaviour
{
    #region Variables
    // TODO: SO 변수를 통해 맵 관련 정보 프리셋 보유
    #endregion

    #region Unity Functions
    private void Start()
    {
        // TODO: 로컬 데이터 베이스 데이터가 있으면 이를 반환
        DVStatus status = new DVStatus(1000, 50, 50);

        DVGolemInfo playerInfo = new DVGolemInfo(status, moveSpeedPoint: 40);
        for (int i = 0; i < 10; i++) 
            DVCubeCreator.Instance.AddRandomGolemCube(playerInfo);
        DVCubeCreator.Instance.CreatePlayer(playerInfo);

        /*DVGolemInfo monsterInfo = new DVGolemInfo(status);
        DVCubeCreator.Instance.CreateMonster(monsterInfo);*/

        StartCoroutine(TempFall());
    }
    #endregion

    #region Coroutines
    // TODO: 장애물 생성 방식 중 하나
    private IEnumerator TempFall()
    {
        DVStatus status = new DVStatus(1, 1, 1);
        const float height = 30f;
        const float range = 30f;

        for (int i = 0; i < 10000; i++)
        {
            var obstacle = DVCubeCreator.Instance.CreateObstacleCube(status);
            float x = Mathf.RoundToInt(Random.Range(-range, range));
            float z = Mathf.RoundToInt(Random.Range(-range, range));
            obstacle.transform.position = new Vector3(x, height, z);

            yield return DVHelper.In.YieldCache.GetWaitForSeconds(0.02f);
        }
    }
    #endregion
}
