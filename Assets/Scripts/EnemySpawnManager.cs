using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    public static EnemySpawnManager Instance { get; private set; }

    [SerializeField] private int _enemyMoveSpeed;
    [Space(10)]
    [SerializeField] private EnemyShipBase[] _enemyShipBase;
    [Space(10)]
    [SerializeField] private EnemysOnLine[] _enemysOnLine;

    private readonly List<EnemyShipBase> _active = new List<EnemyShipBase>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

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

    public int ActiveCount => _active.Count;

    public IReadOnlyList<EnemyShipBase> ActiveEnemies => _active;

    public void RegisterActive(EnemyShipBase enemy)
    {
        if (enemy == null) return;
        if (!_active.Contains(enemy)) _active.Add(enemy);
    }

    public void UnregisterActive(EnemyShipBase enemy)
    {
        if (enemy == null) return;
        _active.Remove(enemy);
    }

    public EnemyShipBase FindNearestToPosition(Vector3 worldPos)
    {
        EnemyShipBase nearest = null;
        float minSqr = float.MaxValue;
        for (int i = 0; i < _active.Count; i++)
        {
            EnemyShipBase e = _active[i];
            if (e == null || e.IsDead) continue;
            Vector3 d = e.transform.position - worldPos;
            d.y = 0f;
            float sqr = d.sqrMagnitude;
            if (sqr < minSqr)
            {
                minSqr = sqr;
                nearest = e;
            }
        }
        return nearest;
    }

    public void FindEnemiesInRadius(Vector3 worldPos, float radius, List<EnemyShipBase> output)
    {
        if (output == null) return;
        output.Clear();
        float sqrRadius = radius * radius;
        for (int i = 0; i < _active.Count; i++)
        {
            EnemyShipBase e = _active[i];
            if (e == null || e.IsDead) continue;
            Vector3 d = e.transform.position - worldPos;
            d.y = 0f;
            if (d.sqrMagnitude <= sqrRadius)
                output.Add(e);
        }
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
                enemyShip.Construct(enemyMoveSpeed, 0);
                enemyShip.onDeath += OnEnemyDeath;
                _enemyList.Add(enemyShip);
            }
        }
    }
    public void StartEnemy()
    {
        if (_enemyList == null || _enemyList.Count == 0) return;

        // Find a ready (inactive, not dead) enemy to avoid spawning a still-dying one.
        int startIdx = Random.Range(0, _enemyList.Count);
        for (int i = 0; i < _enemyList.Count; i++)
        {
            int idx = (startIdx + i) % _enemyList.Count;
            EnemyShipBase candidate = _enemyList[idx];
            if (!candidate.gameObject.activeInHierarchy)
            {
                _enemyList.RemoveAt(idx);
                candidate.gameObject.SetActive(true);
                return;
            }
        }
    }
    private void OnEnemyDeath(EnemyShipBase enemyShipBase) => _enemyList.Add(enemyShipBase);
}
