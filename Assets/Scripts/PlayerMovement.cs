using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Main Movement")]
    [SerializeField] private float _speed = 10f;
    [SerializeField] private Vector3 _direction = Vector3.forward;

    private void Update()
    {
        transform.Translate(_direction.normalized * _speed * Time.deltaTime, Space.World);
    }
}