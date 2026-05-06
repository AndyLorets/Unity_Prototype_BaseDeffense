using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// UI-компонент полоски здоровья. Следует за 3D-целью через WorldToScreenPoint.
public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image _fill;
    [SerializeField] private float _tweenDuration = 0.25f;
    [SerializeField] private Ease _tweenEase = Ease.OutQuad;

    private RectTransform _rect;
    private Camera _camera;
    private Transform _target;
    private Vector3 _offset;
    private bool _follow;
    private Tween _fillTween;

    private void Awake()
    {
        _rect = (RectTransform)transform;
    }

    // Привязывает бар к цели. Камеру пробрасывает пул (избегаем Camera.main каждый кадр).
    public void Bind(Transform target, Vector3 offset, Camera cam)
    {
        _target = target;
        _offset = offset;
        _camera = cam;
        _follow = true;
        gameObject.SetActive(true);
    }

    // Отвязывает и прячет — вызывается пулом при возврате.
    public void Unbind()
    {
        KillTween();
        _follow = false;
        _target = null;
        gameObject.SetActive(false);
    }

    public void UpdateHealth(float current, float max)
    {
        if (_fill == null || max <= 0f) return;

        float target = Mathf.Clamp01(current / max);
        KillTween();
        _fillTween = _fill.DOFillAmount(target, _tweenDuration).SetEase(_tweenEase);
    }

    // Мгновенная установка без анимации — для первичной привязки/респавна.
    public void SetHealthInstant(float current, float max)
    {
        if (_fill == null || max <= 0f) return;
        KillTween();
        _fill.fillAmount = Mathf.Clamp01(current / max);
    }

    private void KillTween()
    {
        if (_fillTween != null && _fillTween.IsActive())
            _fillTween.Kill();
        _fillTween = null;
    }

    private void OnDestroy()
    {
        KillTween();
    }

    private void LateUpdate()
    {
        if (!_follow || _target == null || _camera == null) return;

        Vector3 screenPos = _camera.WorldToScreenPoint(_target.position + _offset);

        // За камерой — прячем (но не освобождаем, владелец живой).
        if (screenPos.z < 0f)
        {
            if (_fill.enabled) _fill.enabled = false;
            return;
        }

        if (!_fill.enabled) _fill.enabled = true;
        _rect.position = screenPos;
    }
}
