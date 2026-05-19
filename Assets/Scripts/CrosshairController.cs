using System;
using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    public enum ScreenSide { None, Left, Right }

    [SerializeField] private Camera _camera;
    [SerializeField] private float _worldY = 0f;
    [SerializeField] internal float _sensitivity = 0.05f;
    [SerializeField] internal float _speed = 10f;
    [SerializeField] private ScreenSide _side = ScreenSide.None;
    [SerializeField] private Vector3 _boundsCenter = Vector3.zero;
    [SerializeField] private Vector3 _boundsSize = new(20f, 0f, 20f);
    [SerializeField] private Vector3 _targetOffset = Vector3.up * 10f;
    [SerializeField] internal bool _useSmoothing = false;
    [SerializeField] internal float _smoothSpeed = 10f;
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private GameSettings _settings;
    [SerializeField] internal float _detectionRadius = 1.5f;

    [Header("Follow Target (Следование за игроком)")]
    [SerializeField] private Transform _followTarget; // СЮДА НУЖНО ПЕРЕТАЩИТЬ ИГРОКА

    public static readonly Color IdleColor = Color.white;
    public static readonly Color TargetColor = Color.red;

    public event Action<Color> OnColorChanged;

    public ITakeDamage CurrentTarget { get; private set; }

    public Vector2 ScreenPosition { get; private set; }

    public Vector3 BoundsCenter => _boundsCenter;
    public Vector3 BoundsSize => _boundsSize;

    private Plane _groundPlane;
    private Vector3 _targetPosition;
    private Vector3 _lastFollowPosition;

    private Vector3 Min
    {
        get
        {
            Vector3 m = _boundsCenter - _boundsSize * 0.5f;
            if (_side == ScreenSide.Right) m.x = _boundsCenter.x;
            return m;
        }
    }

    private Vector3 Max
    {
        get
        {
            Vector3 m = _boundsCenter + _boundsSize * 0.5f;
            if (_side == ScreenSide.Left) m.x = _boundsCenter.x;
            return m;
        }
    }

    private void Awake()
    {
        _groundPlane = new Plane(Vector3.up, new Vector3(0f, _worldY, 0f));
        _targetPosition = transform.position;

        if (_followTarget != null)
            _lastFollowPosition = _followTarget.position;
    }

    private void Start()
    {
        Camera cam = OrientationManager.Instance?.ActiveCamera
                  ?? _camera
                  ?? Camera.main;
        SetCamera(cam);
    }

    private void OnEnable()
    {
        OrientationManager.OnOrientationChanged += OnOrientationChanged;
    }

    private void OnDisable()
    {
        OrientationManager.OnOrientationChanged -= OnOrientationChanged;
    }

    private void OnOrientationChanged(bool portrait)
    {
        if (OrientationManager.Instance?.ActiveCamera != null)
            SetCamera(OrientationManager.Instance.ActiveCamera);
    }

    public void SetCamera(Camera cam)
    {
        _camera = cam;
        if (_camera != null)
            ScreenPosition = (Vector2)_camera.WorldToScreenPoint(transform.position);
    }

    private bool Is3DMode => _settings == null || _settings.crosshairMode == GameSettings.CrosshairMode.World3D;

    public void ApplyRendererMode()
    {
        if (_meshRenderer != null)
            _meshRenderer.enabled = Is3DMode;
    }

    private void Update()
    {
        FollowTarget(); // <--- Добавили сдвиг за игроком
        ApplySmoothing();
        UpdateTarget();
        ApplyRendererMode();
    }

    private void FollowTarget()
    {
        if (_followTarget != null)
        {
            Vector3 delta = _followTarget.position - _lastFollowPosition;
            if (delta.sqrMagnitude > 0.0001f)
            {
                // Смещаем позицию прицела и центр его ограничивающих рамок вслед за игроком
                _targetPosition += delta;
                _boundsCenter += delta;

                if (!_useSmoothing)
                    transform.position += delta;

                _lastFollowPosition = _followTarget.position;
            }
        }
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

    public void ApplyDelta(Vector2 screenDelta)
    {
        if (screenDelta.sqrMagnitude < 0.01f) return;

        Vector2 baseScreenPos = ScreenPosition != Vector2.zero
            ? ScreenPosition
            : (Vector2)_camera.WorldToScreenPoint(_targetPosition);

        Vector3 from = ScreenToWorld(baseScreenPos);
        Vector3 to   = ScreenToWorld(baseScreenPos + screenDelta);
        Vector3 worldDelta = (to - from) * _sensitivity * 100f;

        ApplyWorldDelta(worldDelta, baseScreenPos + screenDelta);
    }

    public void ApplyVelocity(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f) return;

        Vector3 worldDelta = new Vector3(direction.x, 0f, direction.y) * _speed * Time.deltaTime;
        ApplyWorldDelta(worldDelta, _camera != null ? (Vector2)_camera.WorldToScreenPoint(_targetPosition + worldDelta) : ScreenPosition);
    }

    private void ApplyWorldDelta(Vector3 worldDelta, Vector2 newScreenPos)
    {
        _targetPosition += worldDelta;
        _targetPosition.x = Mathf.Clamp(_targetPosition.x, Min.x, Max.x);
        _targetPosition.z = Mathf.Clamp(_targetPosition.z, Min.z, Max.z);
        _targetPosition.y = _worldY;

        if (!_useSmoothing)
        {
            transform.position = _targetPosition;
            ScreenPosition = _camera != null ? (Vector2)_camera.WorldToScreenPoint(transform.position) : newScreenPos;
        }
    }

    private void ApplySmoothing()
    {
        if (!_useSmoothing) return;
        transform.position = Vector3.Lerp(transform.position, _targetPosition, _smoothSpeed * Time.deltaTime);
        if (_camera != null)
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(_boundsCenter, new Vector3(_boundsSize.x, 0.1f, _boundsSize.z));

        Gizmos.color = CurrentTarget != null ? Color.red : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);
        Gizmos.DrawSphere(transform.position, 0.3f);

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