using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private float _worldY = 0f;
    [SerializeField] private float _sensitivity = 0.05f;
    [SerializeField] private Vector3 _boundsCenter = Vector3.zero;
    [SerializeField] private Vector3 _boundsSize = new(20f, 0f, 20f);
    [SerializeField] internal bool _useSmoothing = false;
    [SerializeField] internal float _smoothSpeed = 10f;
    [SerializeField] private MeshRenderer _crosshairRenderer;

    private static readonly Color _idleColor = Color.white;
    private static readonly Color _targetColor = Color.red;

    private Vector2 _lastScreenPos;
    private Plane _groundPlane;
    private Vector3 _targetPosition;
    private TargetController _targetController;

    private Vector3 Min => _boundsCenter - _boundsSize * 0.5f;
    private Vector3 Max => _boundsCenter + _boundsSize * 0.5f;

    private void Awake()
    {
        if (_camera == null) _camera = Camera.main;
        _groundPlane = new Plane(Vector3.up, new Vector3(0f, _worldY, 0f));
        _targetPosition = transform.position;
    }

    private void Start()
    {
        _targetController = ServiceLocator.GetService<TargetController>();
        _targetController.onTargetEnter += OnTargetEnter;
        _targetController.onTargetExit += OnTargetExit;
    }

    private void OnDestroy()
    {
        if (_targetController == null) return;
        _targetController.onTargetEnter -= OnTargetEnter;
        _targetController.onTargetExit -= OnTargetExit;
    }

    private void OnTargetEnter(ITakeDamage _) => SetColor(_targetColor);
    private void OnTargetExit() => SetColor(_idleColor);

    private void SetColor(Color color)
    {
        if (_crosshairRenderer == null) return;
        _crosshairRenderer.material.color = color;
    }

    private void Update()
    {
        HandleInput();
        ApplySmoothing();
    }

    private void HandleInput()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            _lastScreenPos = Input.mousePosition;
        }
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
            {
                _lastScreenPos = touch.position;
            }
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

        Vector3 from = ScreenToWorld(_lastScreenPos - screenDelta);
        Vector3 to   = ScreenToWorld(_lastScreenPos);
        Vector3 worldDelta = (to - from) * _sensitivity * 100f;

        _targetPosition += worldDelta;
        _targetPosition.x = Mathf.Clamp(_targetPosition.x, Min.x, Max.x);
        _targetPosition.z = Mathf.Clamp(_targetPosition.z, Min.z, Max.z);
        _targetPosition.y = _worldY;

        if (!_useSmoothing)
            transform.position = _targetPosition;
    }

    private void ApplySmoothing()
    {
        if (!_useSmoothing) return;
        transform.position = Vector3.Lerp(transform.position, _targetPosition, _smoothSpeed * Time.deltaTime);
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
        Vector3 gizmoSize = new(_boundsSize.x, 0.1f, _boundsSize.z);
        Gizmos.DrawWireCube(_boundsCenter, gizmoSize);
    }
}
