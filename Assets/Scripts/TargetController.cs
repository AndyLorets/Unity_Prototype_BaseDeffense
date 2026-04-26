using UnityEngine;
using DG.Tweening;

public class CrosshairTweenAnimator : MonoBehaviour
{
    [SerializeField] private CrosshairController _crosshair;
    [SerializeField] private IndexInfo _indexInfo;

    private const float def_size = 10f;
    private const float big_size = 15f;
    private const float tween_duration = .6f;

    private Material _mat;
    private Transform _icon;
    private bool _wasAttacking;

    private void Awake()
    {
        _icon = transform.GetChild(0);
        _mat = _icon.GetComponent<MeshRenderer>().material;
    }

    private void Start()
    {
        IdleState();
    }

    private void Update()
    {
        bool isAttacking = _crosshair != null && _crosshair.CurrentTarget != null;

        if (isAttacking && !_wasAttacking)
            AttackState(_crosshair.CurrentTarget);
        else if (!isAttacking && _wasAttacking)
            IdleState();

        _wasAttacking = isAttacking;
    }

    private void AttackState(ITakeDamage takeDamage)
    {
        _icon.DOKill();
        _mat.DOKill();

        float endValue = _icon.localScale.x > def_size ? def_size : big_size;
        _icon.DOScale(Vector3.one * endValue, tween_duration * .3f)
             .OnComplete(() => { if (_wasAttacking) AttackState(takeDamage); });

        Color color = _indexInfo.GetIndexColor(takeDamage.index);
        _mat.DOColor(color, tween_duration * .3f);
    }

    private void IdleState()
    {
        _icon.DOKill();
        _mat.DOKill();

        Vector3 dir = new Vector3(90, _icon.eulerAngles.y + 90, 0);
        _icon.DORotate(dir, tween_duration).OnComplete(() => { if (!_wasAttacking) IdleState(); });

        if (_icon.localScale.x != def_size)
            _icon.DOScale(Vector3.one * def_size, tween_duration * .3f);

        if (_mat.color != Color.white)
            _mat.DOColor(Color.white, tween_duration * .3f);
    }
}
