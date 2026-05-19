using UnityEngine;

public class NearestTargetIndicatorUI : MonoBehaviour
{
    [SerializeField] private WeaponSpecial _weapon;
    [SerializeField] private RectTransform _indicator;
    [SerializeField] private Vector3 _worldOffset = new Vector3(0f, 2.5f, 0f);

    private Camera _camera;
    private EnemyShipBase _currentTarget;

    private void Awake()
    {
        if (_indicator != null)
            _indicator.gameObject.SetActive(false);
    }

    private void Start()
    {
        _camera = OrientationManager.Instance?.ActiveCamera ?? Camera.main;
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
            _camera = OrientationManager.Instance.ActiveCamera;
    }

    private void LateUpdate()
    {
        if (_weapon == null || _indicator == null || _camera == null)
            return;

        EnemyShipBase target = _weapon.FindNearestInActivationZone();

        if (target != _currentTarget)
            _currentTarget = target;

        if (_currentTarget == null || _currentTarget.IsDead)
        {
            _indicator.gameObject.SetActive(false);
            return;
        }

        Vector3 screenPos = _camera.WorldToScreenPoint(_currentTarget.transform.position + _worldOffset);

        if (screenPos.z < 0f)
        {
            _indicator.gameObject.SetActive(false);
            return;
        }

        _indicator.gameObject.SetActive(true);
        _indicator.position = screenPos;
    }
}
