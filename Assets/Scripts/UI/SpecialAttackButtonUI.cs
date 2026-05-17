using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SpecialAttackButtonUI : MonoBehaviour
{
    [SerializeField] private WeaponSpecial _weapon;
    [SerializeField] private AttackKind _kind;
    [SerializeField] private float _disabledAlpha = 0.5f;

    private Button _button;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        _button.onClick.AddListener(OnClick);
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(OnClick);
    }

    private void Update()
    {
        if (_weapon == null) return;

        bool ready = _weapon.IsReady(_kind);
        if (_button.interactable != ready)
            _button.interactable = ready;

        float targetAlpha = ready ? 1f : _disabledAlpha;
        if (!Mathf.Approximately(_canvasGroup.alpha, targetAlpha))
            _canvasGroup.alpha = targetAlpha;
    }

    private void OnClick()
    {
        if (_weapon == null) return;

        switch (_kind)
        {
            case AttackKind.NearestToBase:
                _weapon.TryFireNearestToBase();
                break;
            case AttackKind.CrosshairRadius:
                _weapon.TryFireAtCursorRadius();
                break;
        }
    }
}
