using System;
using UnityEngine;

public class OrientationManager : MonoBehaviour
{
    public static OrientationManager Instance { get; private set; }

    [Header("Cameras")]
    [SerializeField] private Camera _portraitCamera;
    [SerializeField] private Camera _landscapeCamera;

    public static event Action<bool> OnOrientationChanged; // true = portrait

    public bool IsPortrait { get; private set; }
    public Camera ActiveCamera { get; private set; }

    private ScreenOrientation _lastOrientation;
    private bool _lastAspectPortrait;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        bool portrait = Screen.height > Screen.width;
        _lastAspectPortrait = portrait;
        _lastOrientation = Screen.orientation;
        Apply(portrait, instant: true);
    }

    private void Update()
    {
        // Aspect ratio — работает везде: редактор, устройство, AutoRotation
        bool portrait = Screen.height > Screen.width;
        if (portrait != _lastAspectPortrait)
        {
            _lastAspectPortrait = portrait;
            _lastOrientation = Screen.orientation;
            Apply(portrait, instant: false);
        }
    }

    private void Apply(bool portrait, bool instant)
    {
        if (!instant && portrait == IsPortrait) return;

        IsPortrait = portrait;

        if (_portraitCamera != null)
            _portraitCamera.gameObject.SetActive(portrait);

        if (_landscapeCamera != null)
            _landscapeCamera.gameObject.SetActive(!portrait);

        ActiveCamera = portrait ? _portraitCamera : _landscapeCamera;

        // Update HealthBarPool camera reference so bars follow the correct camera
        var pool = ServiceLocator.GetService<HealthBarPool>();
        if (pool != null && ActiveCamera != null)
            pool.SetCamera(ActiveCamera);

        if (!instant)
            OnOrientationChanged?.Invoke(portrait);
    }

#if UNITY_EDITOR
    // Editor helper: toggle orientation preview via context menu
    [ContextMenu("Simulate Portrait")]
    private void SimulatePortrait() => Apply(true, instant: false);

    [ContextMenu("Simulate Landscape")]
    private void SimulateLandscape() => Apply(false, instant: false);
#endif
}
