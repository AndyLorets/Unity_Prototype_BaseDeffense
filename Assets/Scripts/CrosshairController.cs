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
    [SerializeField] private float _shootInterval = 0.3f;
    [SerializeField] private float _shootRadius = 3f;
    [SerializeField] private AudioClip _shootClip;
    [SerializeField] private ParticleSystem _shootEffect;

    private float _shootTimer;
    private bool _isTracking;
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

    private void Update()
    {
        HandleInput();
        ApplySmoothing();
        if (_isTracking) TryShoot();
    }

    private void HandleInput()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            _isTracking = true;
            _lastScreenPos = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            Vector2 current = Input.mousePosition;
            MoveByDelta(current - _lastScreenPos);
            _lastScreenPos = current;
        }
        else
        {
            _isTracking = false;
        }
#else
        if (Input.touchCount > 0)
        {
            Touch touch = Input.touches[Input.touchCount - 1];

            if (touch.phase == TouchPhase.Began)
            {
                _isTracking = true;
                _lastScreenPos = touch.position;
            }
            else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                MoveByDelta(touch.position - _lastScreenPos);
                _lastScreenPos = touch.position;
            }
        }
        else
        {
            _isTracking = false;
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

    private void TryShoot()
    {
        _shootTimer -= Time.deltaTime;
        if (_shootTimer > 0f) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, _shootRadius);
        foreach (Collider hit in hits)
        {
            ITakeDamage target = hit.GetComponent<ITakeDamage>();
            if (target != null)
            {
                target.TakeDamage(9999f, -1);

                if (_shootEffect != null) _shootEffect.Play();
                if (_shootClip != null)
                    ServiceLocator.GetService<AudioManager>().PlayOneShot(_shootClip);

                _shootTimer = _shootInterval;
                return;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 gizmoSize = new(_boundsSize.x, 0.1f, _boundsSize.z);
        Gizmos.DrawWireCube(_boundsCenter, gizmoSize);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _shootRadius);
    }
}
