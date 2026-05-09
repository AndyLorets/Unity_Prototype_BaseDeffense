using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform _target; // Твой игрок

    [Header("Follow Settings")]
    [SerializeField] private Vector3 _offset = new Vector3(0, 10, -15); // Дистанция от игрока
    [SerializeField] private float _smoothSpeed = 0.125f; // Плавность следования
    
    [Header("Axis Constraints")]
    [SerializeField] private bool _followX = true;
    [SerializeField] private bool _followY = false; // Обычно в раннерах высота камеры фиксирована
    [SerializeField] private bool _followZ = true;

    private Vector3 _currentVelocity;

    private void LateUpdate()
    {
        if (_target == null) return;

        // Рассчитываем идеальную позицию (куда камера хочет попасть)
        Vector3 desiredPosition = _target.position + _offset;

        // Если мы не хотим, чтобы камера прыгала вверх-вниз за кораблем (игнорируем покачивание по Y)
        float targetX = _followX ? desiredPosition.x : transform.position.x;
        float targetY = _followY ? desiredPosition.y : transform.position.y;
        float targetZ = _followZ ? desiredPosition.z : transform.position.z;

        Vector3 filteredPosition = new Vector3(targetX, targetY, targetZ);

        // Плавное перемещение к этой позиции
        // Используем SmoothDamp для самого мягкого эффекта следования
        transform.position = Vector3.SmoothDamp(transform.position, filteredPosition, ref _currentVelocity, _smoothSpeed);

        // Камера всегда смотрит на игрока (можно закомментировать, если нужен фиксированный угол)
        // transform.LookAt(_target.position + Vector3.up * 2f); 
    }
}