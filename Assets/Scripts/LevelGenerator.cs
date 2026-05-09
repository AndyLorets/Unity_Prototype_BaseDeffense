using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ObjectPool _pool;
    [SerializeField] private Transform _playerTransform;

    [Header("Settings")]
    [SerializeField] private float _terrainSize = 500f;
    [SerializeField] private int _tilesOnScreen = 3;
    [SerializeField] private Vector3 _spawnOffset; // Смещение террейна (например, X = -250)
    
    [Header("Movement Direction")]
    [SerializeField] private Vector3 _moveDirection = Vector3.forward; // Направление полета

    private float _spawnZ = 0f;
    private List<GameObject> _activeTiles = new List<GameObject>();

    private void Start()
    {
        // Устанавливаем начальную точку спавна там, где стоит игрок (или в 0)
        _spawnZ = _playerTransform.position.z;

        for (int i = 0; i < _tilesOnScreen; i++)
        {
            SpawnTile();
        }
    }

    private void Update()
    {
        // Проверяем дистанцию в зависимости от направления
        // Если летим вперед (Z+), проверяем перелет за край. Если назад (Z-), условие инвертируется.
        float playerZ = _playerTransform.position.z;
        float firstTileZ = _activeTiles[0].transform.position.z;

        bool shouldLoop = _moveDirection.z >= 0 
            ? playerZ > (firstTileZ + _terrainSize) // Для полета вперед
            : playerZ < (firstTileZ - _terrainSize); // Для полета назад

        if (shouldLoop)
        {
            RemoveOldTile();
            SpawnTile();
        }
    }

    private void SpawnTile()
    {
        // Позиция: текущий Z + заданный Offset
        Vector3 spawnPos = new Vector3(0, 0, _spawnZ) + _spawnOffset;
        GameObject tile = _pool.GetFromPool(spawnPos);
        _activeTiles.Add(tile);
        
        // Сдвигаем точку следующего спавна согласно направлению движения
        _spawnZ += (_moveDirection.z >= 0) ? _terrainSize : -_terrainSize;
    }

    private void RemoveOldTile()
    {
        _pool.ReturnToPool(_activeTiles[0]);
        _activeTiles.RemoveAt(0);
    }
}