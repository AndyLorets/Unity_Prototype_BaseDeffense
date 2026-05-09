using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject _prefab;
    [SerializeField] private int _initialSize = 5;
    
    private Queue<GameObject> _pool = new Queue<GameObject>();

    private void Awake()
    {
        for (int i = 0; i < _initialSize; i++)
        {
            GenerateNewObject();
        }
    }

    private GameObject GenerateNewObject()
    {
        GameObject obj = Instantiate(_prefab, transform);
        obj.SetActive(false);
        _pool.Enqueue(obj);
        return obj;
    }

    public GameObject GetFromPool(Vector3 position)
    {
        GameObject obj = _pool.Count > 0 ? _pool.Dequeue() : GenerateNewObject();
        
        obj.transform.position = position;
        obj.SetActive(true);
        return obj;
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        _pool.Enqueue(obj);
    }
}