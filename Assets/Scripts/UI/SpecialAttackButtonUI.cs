using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SpecialAttackButtonUI : MonoBehaviour
{
    [SerializeField] private WeaponSpecial _weapon;
    [SerializeField] private AttackKind _kind;
    [SerializeField] private float _disabledAlpha = 0.5f;
    [SerializeField] private Image _cooldownFill;

    private Button _button;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        _button.onClick.AddListener(OnClick);

        if (_cooldownFill != null)
            _cooldownFill.fillAmount = 0f;
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

        if (_cooldownFill != null)
        {
            float cooldown = _kind == AttackKind.NearestToBase
                ? _weapon.GetCooldownRemaining(AttackKind.NearestToBase)
                : _weapon.GetCooldownRemaining(AttackKind.CrosshairRadius);

            float total = _kind == AttackKind.NearestToBase
                ? _weapon.NearestCooldown
                : _weapon.RadiusCooldown;

            _cooldownFill.fillAmount = total > 0f ? cooldown / total : 0f;
        }
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
