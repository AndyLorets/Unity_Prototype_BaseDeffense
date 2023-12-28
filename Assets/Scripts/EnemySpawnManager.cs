using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    [SerializeField] private int _enemyMoveSpeed;
    [Space(10)]
    [SerializeField] private EnemyShipBase[] _enemyShipBase;
    [Space(10)]
    [SerializeField] private EnemysOnLine[] _enemysOnLine;

    private void Start()
    {
        Construct();
        StartCoroutine(EnemyStarter());
    }
    private void Construct()
    {
        for (int i = 0; i < _enemysOnLine.Length; i++)
            _enemysOnLine[i].InitEnemys(_enemyShipBase, transform, _enemyMoveSpeed); 
    }   
    private IEnumerator EnemyStarter()
    {
        while(true)
        {
            _enemysOnLine[Random.Range(0, _enemysOnLine.Length)].StartEnemy();
            yield return new WaitForSeconds(2); 
        }
    }
    public static EnemyShipBase GetInstEnemyShip(EnemyShipBase enemyShip)
    {
        return Instantiate(enemyShip); 
    }
}

[System.Serializable]
public class EnemysOnLine
{
    [SerializeField] private int _localPosX;
    private List<EnemyShipBase> _enemyList = new List<EnemyShipBase>();

    private const int enemy_spawn_count = 10; 

    public void InitEnemys(EnemyShipBase[] enemyShipBases, Transform parent, float enemyMoveSpeed)
    {
        for (int i = 0; i < enemy_spawn_count; i++)
        {
            for (int ii = 0; ii < enemyShipBases.Length; ii++)
            {
                EnemyShipBase enemyShip = EnemySpawnManager.GetInstEnemyShip(enemyShipBases[ii]);
                enemyShip.transform.parent = parent;
                enemyShip.transform.localPosition = new Vector3(_localPosX, 0,0);
                enemyShip.transform.localEulerAngles = new Vector3(0, -90, 0);
                enemyShip.gameObject.SetActive(false);
                enemyShip.Construct(enemyMoveSpeed, Random.Range(0, IndexInfo.index_count));
                enemyShip.onDeath += OnEnemyDeath; 
                _enemyList.Add(enemyShip);
            }
        }
    }
    public void StartEnemy()
    {
        if (_enemyList == null) return; 

        int r = Random.Range(0, _enemyList.Count);
        _enemyList[r].gameObject.SetActive(true);
        _enemyList.RemoveAt(r);
    }
    private void OnEnemyDeath(EnemyShipBase enemyShipBase) => _enemyList.Add(enemyShipBase);
}
