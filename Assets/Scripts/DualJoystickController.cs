using UnityEngine;

public class DualJoystickController : MonoBehaviour
{
    public enum InputMode { Delta, Velocity }
    public enum HandSide { Right, Left }

    [Header("Input mode")]
    [SerializeField] private InputMode _mode = InputMode.Velocity;

    public InputMode Mode
    {
        get => _mode;
        set { _mode = value; ApplyModeVisuals(); }
    }

    [Header("Hand side")]
    [SerializeField] private HandSide _handSide = HandSide.Right;

    public HandSide Hand
    {
        get => _handSide;
        set => _handSide = value;
    }

    [Header("Crosshair")]
    [SerializeField] private CrosshairController _crosshair;

    [Header("Velocity mode")]
    [SerializeField] private FixedJoystick _fixedJoystick;

    [Header("Delta mode")]
    [SerializeField] private VariableJoystick _floatingJoystick;
    [SerializeField] private float _deltaScale = 1f;

    private int _fingerId = -1;
    private Vector2 _lastPos;

    private void Awake()
    {
        ApplyModeVisuals();
    }

    private void OnValidate()
    {
        if (Application.isPlaying) ApplyModeVisuals();
    }

    private void ApplyModeVisuals()
    {
        bool velocity = _mode == InputMode.Velocity;
        if (_fixedJoystick != null) _fixedJoystick.gameObject.SetActive(velocity);
        if (_floatingJoystick != null) _floatingJoystick.gameObject.SetActive(!velocity);
    }

    private void Update()
    {
        if (_mode == InputMode.Velocity)
            UpdateVelocity();
        else
            UpdateDelta();
    }

    private void UpdateVelocity()
    {
        if (_crosshair != null && _fixedJoystick != null)
            _crosshair.ApplyVelocity(_fixedJoystick.Direction);
    }

    private bool IsInHandHalf(Vector2 screenPos)
    {
        float halfWidth = Screen.width * 0.5f;
        return _handSide == HandSide.Right ? screenPos.x >= halfWidth : screenPos.x < halfWidth;
    }

    private void UpdateDelta()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 pos = Input.mousePosition;
            if (IsInHandHalf(pos))
                _lastPos = pos;
        }
        else if (Input.GetMouseButton(0))
        {
            Vector2 pos = Input.mousePosition;
            if (IsInHandHalf(pos))
            {
                Vector2 delta = pos - _lastPos;
                if (_crosshair != null) _crosshair.ApplyDelta(delta * _deltaScale);
                _lastPos = pos;
            }
        }
#else
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.touches[i];
            bool inHandHalf = IsInHandHalf(touch.position);

            if (touch.phase == TouchPhase.Began)
            {
                if (inHandHalf && _fingerId == -1)
                {
                    _fingerId = touch.fingerId;
                    _lastPos = touch.position;
                }
            }
            else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                if (touch.fingerId == _fingerId && _crosshair != null)
                {
                    Vector2 delta = touch.position - _lastPos;
                    _crosshair.ApplyDelta(delta * _deltaScale);
                    _lastPos = touch.position;
                }
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                if (touch.fingerId == _fingerId) _fingerId = -1;
            }
        }
#endif
    }
}
