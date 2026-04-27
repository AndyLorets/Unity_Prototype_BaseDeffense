using System;
using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private float _worldY = 0f;
    [SerializeField] private float _sensitivity = 0.05f;
    [SerializeField] private Vector3 _boundsCenter = Vector3.zero;
    [SerializeField] private Vector3 _boundsSize = new(20f, 0f, 20f);
    [SerializeField] private Vector3 _targetOffset = Vector3.up * 10f;
    [SerializeField] internal bool _useSmoothing = false;
    [SerializeField] internal float _smoothSpeed = 10f;
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private GameSettings _settings;
    [SerializeField] private float _detectionRadius = 1.5f;

    public static readonly Color IdleColor = Color.white;
    public static readonly Color TargetColor = Color.red;

    public event Action<Color> OnColorChanged;

    public ITakeDamage CurrentTarget { get; private set; }

    // Raw screen position of the crosshair — use this for UI to avoid perspective offset
    public Vector2 ScreenPosition { get; private set; }

    private Vector2 _lastScreenPos;
    private Plane _groundPlane;
    private Vector3 _targetPosition;

    private Vector3 Min => _boundsCenter - _boundsSize * 0.5f;
    private Vector3 Max => _boundsCenter + _boundsSize * 0.5f;

    private void Awake()
    {
        if (_camera == null) _camera = Camera.main;
        _groundPlane = new Plane(Vector3.up, new Vector3(0f, _worldY, 0f));
        _targetPosition = transform.position;
    }

    private bool Is3DMode => _settings == null || _settings.crosshairMode == GameSettings.CrosshairMode.World3D;

    public void ApplyRendererMode()
    {
        if (_meshRenderer != null)
            _meshRenderer.enabled = Is3DMode;
    }

    private void Update()
    {
        HandleInput();
        ApplySmoothing();
        UpdateTarget();
        ApplyRendererMode();
    }

    private void UpdateTarget()
    {
        ITakeDamage previous = CurrentTarget;
        CurrentTarget = null;

        if (_camera != null && ScreenPosition != Vector2.zero)
        {
            Ray ray = !Is3DMode ?_camera.ScreenPointToRay(ScreenPosition) : new Ray(transform.position + _targetOffset, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, _detectionRadius * 100f))
                CurrentTarget = hit.collider.GetComponent<ITakeDamage>();
        }

        if (CurrentTarget != previous)
        {
            bool hasTarget = CurrentTarget != null;
            Color color = hasTarget ? TargetColor : IdleColor;
            if (_meshRenderer != null) _meshRenderer.material.color = color;
            OnColorChanged?.Invoke(color);
        }
    }

    private void HandleInput()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
            _lastScreenPos = Input.mousePosition;
        else if (Input.GetMouseButton(0))
        {
            Vector2 current = Input.mousePosition;
            MoveByDelta(current - _lastScreenPos);
            _lastScreenPos = current;
        }
#else
        if (Input.touchCount > 0)
        {
            Touch touch = Input.touches[Input.touchCount - 1];
            if (touch.phase == TouchPhase.Began)
                _lastScreenPos = touch.position;
            else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                MoveByDelta(touch.position - _lastScreenPos);
                _lastScreenPos = touch.position;
            }
        }
#endif
    }

    private void MoveByDelta(Vector2 screenDelta)
    {
        if (screenDelta.sqrMagnitude < 0.01f) return;

        Vector3 from = ScreenToWorld(_lastScreenPos);
        Vector3 to   = ScreenToWorld(_lastScreenPos + screenDelta);
        Vector3 worldDelta = (to - from) * _sensitivity * 100f;

        _targetPosition += worldDelta;
        _targetPosition.x = Mathf.Clamp(_targetPosition.x, Min.x, Max.x);
        _targetPosition.z = Mathf.Clamp(_targetPosition.z, Min.z, Max.z);
        _targetPosition.y = _worldY;

        if (!_useSmoothing)
        {
            transform.position = _targetPosition;
            ScreenPosition = _lastScreenPos;
        }
    }

    private void ApplySmoothing()
    {
        if (!_useSmoothing) return;
        transform.position = Vector3.Lerp(transform.position, _targetPosition, _smoothSpeed * Time.deltaTime);
        ScreenPosition = _camera.WorldToScreenPoint(transform.position);
    }

    private Vector3 ScreenToWorld(Vector2 screenPos)
    {
        Ray ray = _camera.ScreenPointToRay(screenPos);
        _groundPlane.Raycast(ray, out float distance);
        return ray.GetPoint(distance);
    }

    private void OnDrawGizmosSelected()
    {
        // Bounds
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(_boundsCenter, new Vector3(_boundsSize.x, 0.1f, _boundsSize.z));

        // Crosshair position sphere + detection radius
        Gizmos.color = CurrentTarget != null ? Color.red : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);
        Gizmos.DrawSphere(transform.position, 0.3f);

        // Ray from camera through screen position
        if (_camera != null && Application.isPlaying && ScreenPosition != Vector2.zero)
        {
            Ray ray = !Is3DMode ?_camera.ScreenPointToRay(ScreenPosition) : new Ray(transform.position + _targetOffset, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, _detectionRadius * 100f))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(ray.origin, hit.point);
                Gizmos.DrawWireSphere(hit.point, 0.5f);
            }
            else if (_groundPlane.Raycast(ray, out float dist))
            {
                Vector3 groundPoint = ray.GetPoint(dist);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(ray.origin, groundPoint);
                Gizmos.DrawWireSphere(groundPoint, 0.2f);
            }
        }
    }
}
