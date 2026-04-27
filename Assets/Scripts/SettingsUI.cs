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
    [SerializeField] private CrosshairController _crosshair;

    [Header("Crosshair Sliders")]
    [SerializeField] private Slider _sensitivitySlider;
    [SerializeField] private TMP_Text _sensitivityLabel;
    [SerializeField] private Slider _smoothSpeedSlider;
    [SerializeField] private TMP_Text _smoothSpeedLabel;
    [SerializeField] private Slider _detectionRadiusSlider;
    [SerializeField] private TMP_Text _detectionRadiusLabel;

    private void Start()
    {
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        var group = gameObject.AddComponent<ToggleGroup>();
        group.allowSwitchOff = false;
        _toggle3D.group = group;
        _toggleUI.group = group;

        _toggle3D.onValueChanged.AddListener(on => { if (on) SetMode(GameSettings.CrosshairMode.World3D); });
        _toggleUI.onValueChanged.AddListener(on => { if (on) SetMode(GameSettings.CrosshairMode.ScreenUI); });

        Refresh();

        if (_sensitivitySlider != null)
            _sensitivitySlider.onValueChanged.AddListener(v =>
            {
                _crosshair._sensitivity = v;
                if (_sensitivityLabel != null) _sensitivityLabel.text = v.ToString("F2");
            });

        if (_smoothSpeedSlider != null)
            _smoothSpeedSlider.onValueChanged.AddListener(v =>
            {
                _crosshair._smoothSpeed = v;
                if (_smoothSpeedLabel != null) _smoothSpeedLabel.text = v.ToString("F1");
            });

        if (_detectionRadiusSlider != null)
            _detectionRadiusSlider.onValueChanged.AddListener(v =>
            {
                _crosshair._detectionRadius = v;
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
        _settings.crosshairMode = mode;
        ApplyMode();
    }

    private void ApplyMode()
    {
        bool is3D = _settings.crosshairMode == GameSettings.CrosshairMode.World3D;
        _crosshairUIObject?.SetActive(!is3D);
    }

    private void Refresh()
    {
        bool is3D = _settings.crosshairMode == GameSettings.CrosshairMode.World3D;
        _toggle3D.SetIsOnWithoutNotify(is3D);
        _toggleUI.SetIsOnWithoutNotify(!is3D);
        ApplyMode();

        if (_crosshair == null) return;

        if (_sensitivitySlider != null)
        {
            _sensitivitySlider.minValue = 0.01f;
            _sensitivitySlider.maxValue = Mathf.Max(0.5f, _crosshair._sensitivity * 4f);
            _sensitivitySlider.SetValueWithoutNotify(_crosshair._sensitivity);
            if (_sensitivityLabel != null) _sensitivityLabel.text = _crosshair._sensitivity.ToString("F2");
        }
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
