using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private GameSettings _settings;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Toggle _toggle3D;
    [SerializeField] private Toggle _toggleUI;
    [SerializeField] private float _fadeDuration = 0.3f;
    [SerializeField] private GameObject _crosshairUIObject;
    [SerializeField] private GameObject _crosshairUIObjectRight;
    [SerializeField] private CrosshairController _crosshair;
    [SerializeField] private CrosshairController _crosshairRight;
    [SerializeField] private DualJoystickController _dualJoystick;

    [Header("Crosshair Sliders")]
    [SerializeField] private Slider _sensitivitySlider;
    [SerializeField] private TMP_Text _sensitivityLabel;
    [SerializeField] private Slider _smoothSpeedSlider;
    [SerializeField] private TMP_Text _smoothSpeedLabel;
    [SerializeField] private Slider _detectionRadiusSlider;
    [SerializeField] private TMP_Text _detectionRadiusLabel;
    [SerializeField] private Slider _speedSlider;
    [SerializeField] private TMP_Text _speedLabel;
    [Tooltip("Optional: parent row of Speed slider; falls back to slider's own GameObject if empty.")]
    [SerializeField] private GameObject _speedSliderGroup;
    [Tooltip("Optional: parent row of Sensitivity slider; falls back to slider's own GameObject if empty.")]
    [SerializeField] private GameObject _sensitivitySliderGroup;
    [Tooltip("Optional: parent row of SmoothSpeed slider; falls back to slider's own GameObject if empty.")]
    [SerializeField] private GameObject _smoothSpeedSliderGroup;

    [Header("Input Mode Toggles")]
    [SerializeField] private Toggle _toggleVelocity;
    [SerializeField] private Toggle _toggleDelta;

    [Header("Smoothing Toggle")]
    [SerializeField] private Toggle _useSmoothingToggle;

    private void Start()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        // 3D / UI mode — pair toggles, no ToggleGroup, we drive visuals manually.
        BindPair(_toggle3D, _toggleUI,
            () => SetMode(GameSettings.CrosshairMode.World3D),
            () => SetMode(GameSettings.CrosshairMode.ScreenUI));

        // Velocity / Delta input mode.
        BindPair(_toggleVelocity, _toggleDelta,
            () => SetInputMode(DualJoystickController.InputMode.Velocity),
            () => SetInputMode(DualJoystickController.InputMode.Delta));

        // Use Smoothing — single toggle, flips state on every click.
        if (_useSmoothingToggle != null)
        {
            _useSmoothingToggle.onValueChanged.RemoveAllListeners();
            _useSmoothingToggle.onValueChanged.AddListener(_ =>
            {
                bool newValue = !(_crosshair != null && _crosshair._useSmoothing);
                Debug.Log($"[SettingsUI] UseSmoothing toggled → {newValue}");
                ApplyToBothCrosshairs(c => c._useSmoothing = newValue);
                SetCheckmark(_useSmoothingToggle, newValue);
                ApplySliderVisibility();
            });
        }

        // Sync UI with current crosshair state BEFORE attaching listeners,
        // so the slider's initial value doesn't overwrite the crosshair's real value.
        Refresh();

        if (_sensitivitySlider != null)
            _sensitivitySlider.onValueChanged.AddListener(v =>
            {
                ApplyToBothCrosshairs(c => c._sensitivity = v);
                if (_sensitivityLabel != null) _sensitivityLabel.text = v.ToString("F2");
            });

        if (_speedSlider != null)
            _speedSlider.onValueChanged.AddListener(v =>
            {
                ApplyToBothCrosshairs(c => c._speed = v);
                if (_speedLabel != null) _speedLabel.text = v.ToString("F1");
            });

        if (_smoothSpeedSlider != null)
            _smoothSpeedSlider.onValueChanged.AddListener(v =>
            {
                ApplyToBothCrosshairs(c => c._smoothSpeed = v);
                if (_smoothSpeedLabel != null) _smoothSpeedLabel.text = v.ToString("F1");
            });

        if (_detectionRadiusSlider != null)
            _detectionRadiusSlider.onValueChanged.AddListener(v =>
            {
                ApplyToBothCrosshairs(c => c._detectionRadius = v);
                if (_detectionRadiusLabel != null) _detectionRadiusLabel.text = v.ToString("F1");
            });
    }

    public void Open()
    {
        Time.timeScale = 0f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.DOKill();
        _canvasGroup.DOFade(1f, _fadeDuration).SetUpdate(true);
    }

    public void Close()
    {
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.DOKill();
        _canvasGroup.DOFade(0f, _fadeDuration).SetUpdate(true)
            .OnComplete(() => Time.timeScale = 1f);
    }

    private void SetMode(GameSettings.CrosshairMode mode)
    {
        if (_settings != null) _settings.crosshairMode = mode;
        ApplyMode();
    }

    private void SetInputMode(DualJoystickController.InputMode mode)
    {
        if (_dualJoystick != null) _dualJoystick.Mode = mode;
        ApplySliderVisibility();
    }

    private void ApplySliderVisibility()
    {
        bool velocity = _dualJoystick != null && _dualJoystick.Mode == DualJoystickController.InputMode.Velocity;
        bool smoothing = _crosshair != null && _crosshair._useSmoothing;

        GameObject speedGroup = _speedSliderGroup != null
            ? _speedSliderGroup
            : (_speedSlider != null ? _speedSlider.gameObject : null);
        GameObject sensitivityGroup = _sensitivitySliderGroup != null
            ? _sensitivitySliderGroup
            : (_sensitivitySlider != null ? _sensitivitySlider.gameObject : null);
        GameObject smoothSpeedGroup = _smoothSpeedSliderGroup != null
            ? _smoothSpeedSliderGroup
            : (_smoothSpeedSlider != null ? _smoothSpeedSlider.gameObject : null);

        if (speedGroup != null) speedGroup.SetActive(velocity);
        if (sensitivityGroup != null) sensitivityGroup.SetActive(!velocity);
        if (smoothSpeedGroup != null) smoothSpeedGroup.SetActive(smoothing);
    }

    // Wires two toggles as a mutually-exclusive pair without using Unity's ToggleGroup.
    // Whichever was clicked becomes "on" and the other becomes "off" — visuals driven manually.
    private void BindPair(Toggle first, Toggle second, System.Action onFirst, System.Action onSecond)
    {
        if (first == null || second == null) return;

        // Detach from any ToggleGroup that might interfere.
        first.group = null;
        second.group = null;

        first.onValueChanged.RemoveAllListeners();
        second.onValueChanged.RemoveAllListeners();

        first.onValueChanged.AddListener(on =>
        {
            Debug.Log($"[SettingsUI] {first.name} clicked → {on}");
            // Force "on" — single click always picks this one.
            SetCheckmark(first, true);
            SetCheckmark(second, false);
            onFirst?.Invoke();
        });
        second.onValueChanged.AddListener(on =>
        {
            Debug.Log($"[SettingsUI] {second.name} clicked → {on}");
            SetCheckmark(second, true);
            SetCheckmark(first, false);
            onSecond?.Invoke();
        });
    }

    // Sets toggle visual + internal state without firing onValueChanged.
    private static void SetCheckmark(Toggle toggle, bool on)
    {
        if (toggle == null) return;
        toggle.SetIsOnWithoutNotify(on);

        // Try Toggle.graphic first.
        Graphic g = toggle.graphic;
        GameObject checkmark = g != null ? g.gameObject : null;
        string source = checkmark != null ? "Toggle.graphic" : "(none)";

        // Fallback: child named "Checkmark" or "Background/Checkmark".
        if (checkmark == null)
        {
            Transform t = toggle.transform.Find("Checkmark") ?? toggle.transform.Find("Background/Checkmark");
            if (t != null) { checkmark = t.gameObject; source = "child:Checkmark"; }
        }

        if (checkmark != null)
        {
            checkmark.SetActive(on);
            Debug.Log($"[SettingsUI] SetCheckmark '{toggle.name}' → {on} (via {source}, '{checkmark.name}' active={checkmark.activeSelf})");
        }
        else
            Debug.LogWarning($"[SettingsUI] Toggle '{toggle.name}' has no Graphic/Checkmark — cannot update visual.");
    }

    private void ApplyMode()
    {
        bool is3D = _settings != null && _settings.crosshairMode == GameSettings.CrosshairMode.World3D;
        if (_crosshairUIObject != null) _crosshairUIObject.SetActive(!is3D);
        if (_crosshairUIObjectRight != null) _crosshairUIObjectRight.SetActive(!is3D);
    }

    private void ApplyToBothCrosshairs(System.Action<CrosshairController> action)
    {
        if (_crosshair != null) action(_crosshair);
        if (_crosshairRight != null) action(_crosshairRight);
    }

    private void Refresh()
    {
        Debug.Log($"[SettingsUI] Refresh — settings={_settings}, dualJoystick={_dualJoystick}, crosshair={_crosshair}, useSmoothingToggle={_useSmoothingToggle}");

        bool is3D = _settings != null && _settings.crosshairMode == GameSettings.CrosshairMode.World3D;
        SetCheckmark(_toggle3D, is3D);
        SetCheckmark(_toggleUI, !is3D);
        ApplyMode();

        if (_dualJoystick != null)
        {
            bool velocity = _dualJoystick.Mode == DualJoystickController.InputMode.Velocity;
            Debug.Log($"[SettingsUI] DualJoystick.Mode = {_dualJoystick.Mode} → velocity toggle = {velocity}");
            SetCheckmark(_toggleVelocity, velocity);
            SetCheckmark(_toggleDelta, !velocity);
        }
        else
        {
            Debug.LogWarning("[SettingsUI] _dualJoystick is NULL — Velocity/Delta toggles will not be initialized.");
        }

        ApplySliderVisibility();

        if (_crosshair == null)
        {
            Debug.LogWarning("[SettingsUI] _crosshair is NULL — UseSmoothing toggle will not be initialized.");
            return;
        }

        Debug.Log($"[SettingsUI] Crosshair._useSmoothing = {_crosshair._useSmoothing}");
        SetCheckmark(_useSmoothingToggle, _crosshair._useSmoothing);

        if (_sensitivitySlider != null)
        {
            _sensitivitySlider.minValue = 0.01f;
            _sensitivitySlider.maxValue = Mathf.Max(0.5f, _crosshair._sensitivity * 4f);
            _sensitivitySlider.SetValueWithoutNotify(_crosshair._sensitivity);
            if (_sensitivityLabel != null) _sensitivityLabel.text = _crosshair._sensitivity.ToString("F2");
        }
        if (_speedSlider != null)
        {
            _speedSlider.minValue = 1f;
            _speedSlider.maxValue = Mathf.Max(100f, _crosshair._speed * 2f);
            _speedSlider.SetValueWithoutNotify(_crosshair._speed);
            if (_speedLabel != null) _speedLabel.text = _crosshair._speed.ToString("F1");
            Debug.Log($"[SettingsUI] SpeedSlider '{_speedSlider.name}' init → value={_speedSlider.value}, min={_speedSlider.minValue}, max={_speedSlider.maxValue}, whole={_speedSlider.wholeNumbers}, crosshair._speed={_crosshair._speed}");
        }
        if (_smoothSpeedSlider != null)
            Debug.Log($"[SettingsUI] SmoothSpeedSlider name='{_smoothSpeedSlider.name}' (assigned to _smoothSpeedSlider field)");
        if (_smoothSpeedSlider != null)
        {
            _smoothSpeedSlider.minValue = 1f;
            _smoothSpeedSlider.maxValue = Mathf.Max(30f, _crosshair._smoothSpeed * 2f);
            _smoothSpeedSlider.SetValueWithoutNotify(_crosshair._smoothSpeed);
            if (_smoothSpeedLabel != null) _smoothSpeedLabel.text = _crosshair._smoothSpeed.ToString("F1");
        }
        if (_detectionRadiusSlider != null)
        {
            _detectionRadiusSlider.minValue = 0.5f;
            _detectionRadiusSlider.maxValue = Mathf.Max(20f, _crosshair._detectionRadius * 2f);
            _detectionRadiusSlider.SetValueWithoutNotify(_crosshair._detectionRadius);
            if (_detectionRadiusLabel != null) _detectionRadiusLabel.text = _crosshair._detectionRadius.ToString("F1");
        }
    }
}
