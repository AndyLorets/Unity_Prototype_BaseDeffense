using UnityEngine;
using UnityEngine.EventSystems;

public class DualJoystickController : MonoBehaviour
{
    public enum InputMode { Delta, Velocity }

    [Header("Input mode")]
    [SerializeField] private InputMode _mode = InputMode.Velocity;

    public InputMode Mode
    {
        get => _mode;
        set { _mode = value; ApplyModeVisuals(); }
    }

    [Header("Crosshairs")]
    [SerializeField] private CrosshairController _leftCrosshair;
    [SerializeField] private CrosshairController _rightCrosshair;

    [Header("Velocity mode")]
    [SerializeField] private FixedJoystick _leftFixedJoystick;
    [SerializeField] private FixedJoystick _rightFixedJoystick;

    [Header("Delta mode")]
    [SerializeField] private VariableJoystick _leftFloatingJoystick;
    [SerializeField] private VariableJoystick _rightFloatingJoystick;
    [SerializeField] private float _deltaScale = 1f;

    private int _leftFingerId = -1;
    private int _rightFingerId = -1;
    private Vector2 _leftLastPos;
    private Vector2 _rightLastPos;

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
        if (_leftFixedJoystick != null) _leftFixedJoystick.gameObject.SetActive(velocity);
        if (_rightFixedJoystick != null) _rightFixedJoystick.gameObject.SetActive(velocity);
        if (_leftFloatingJoystick != null) _leftFloatingJoystick.gameObject.SetActive(!velocity);
        if (_rightFloatingJoystick != null) _rightFloatingJoystick.gameObject.SetActive(!velocity);
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
        if (_leftCrosshair != null && _leftFixedJoystick != null)
            _leftCrosshair.ApplyVelocity(_leftFixedJoystick.Direction);

        if (_rightCrosshair != null && _rightFixedJoystick != null)
            _rightCrosshair.ApplyVelocity(_rightFixedJoystick.Direction);
    }

    private void UpdateDelta()
    {
        float halfWidth = Screen.width * 0.5f;

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 pos = Input.mousePosition;
            if (pos.x < halfWidth)
                _leftLastPos = pos;
            else
                _rightLastPos = pos;
        }
        else if (Input.GetMouseButton(0))
        {
            Vector2 pos = Input.mousePosition;
            if (pos.x < halfWidth)
            {
                Vector2 delta = pos - _leftLastPos;
                if (_leftCrosshair != null) _leftCrosshair.ApplyDelta(delta * _deltaScale);
                _leftLastPos = pos;
            }
            else
            {
                Vector2 delta = pos - _rightLastPos;
                if (_rightCrosshair != null) _rightCrosshair.ApplyDelta(delta * _deltaScale);
                _rightLastPos = pos;
            }
        }
#else
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.touches[i];
            bool isLeft = touch.position.x < halfWidth;

            if (touch.phase == TouchPhase.Began)
            {
                if (isLeft && _leftFingerId == -1)
                {
                    _leftFingerId = touch.fingerId;
                    _leftLastPos = touch.position;
                }
                else if (!isLeft && _rightFingerId == -1)
                {
                    _rightFingerId = touch.fingerId;
                    _rightLastPos = touch.position;
                }
            }
            else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                if (touch.fingerId == _leftFingerId && _leftCrosshair != null)
                {
                    Vector2 delta = touch.position - _leftLastPos;
                    _leftCrosshair.ApplyDelta(delta * _deltaScale);
                    _leftLastPos = touch.position;
                }
                else if (touch.fingerId == _rightFingerId && _rightCrosshair != null)
                {
                    Vector2 delta = touch.position - _rightLastPos;
                    _rightCrosshair.ApplyDelta(delta * _deltaScale);
                    _rightLastPos = touch.position;
                }
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                if (touch.fingerId == _leftFingerId) _leftFingerId = -1;
                else if (touch.fingerId == _rightFingerId) _rightFingerId = -1;
            }
        }
#endif
    }
}
