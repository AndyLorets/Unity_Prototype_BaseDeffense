using DG.Tweening;
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
    }
}
