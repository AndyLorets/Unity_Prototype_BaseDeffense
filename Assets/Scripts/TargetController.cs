using UnityEngine;
using DG.Tweening;
using System;

public class TargetController : MonoBehaviour
{
    [SerializeField] private IndexInfo _indexInfo; 

    private const float radius = 20f;
    private const float moveDuration = .1f;

    private float _curentValueX;

    public Action<ITakeDamage> onTargetEnter;
    public Action onTargetExit;

    private const float def_size = 10f;
    private const float big_size = 15f;
    private const float tween_duration = .6f;

    private Material _mat;
    private int _targetIndex;

    private Transform _icon; 
    private Collider _targetColl; 

    private void OnEnable()
    {
        onTargetEnter += AttackState;
        onTargetExit += IdleState;

        SwipeController.onSwipeLeft += MoveLeft;
        SwipeController.onSwipeRight += MoveRight;
    }
    private void OnDisable()
    {
        onTargetEnter -= AttackState;
        onTargetExit -= IdleState;
    }

    private void Awake()
    {
        ServiceLocator.RegisterService(this);
        _icon = transform.GetChild(0);
        _mat = _icon.GetComponent<MeshRenderer>().material;
    }
    private void Start()
    {
        IdleState(); 
    }
    private void MoveLeft()
    {
        if (_curentValueX <= -radius) return;

        ServiceLocator.GetService<AudioManager>().PlayTargetAudio();

        float endValue = _curentValueX - radius;
        _curentValueX = endValue;
        transform.DOKill(); 
        transform.DOMoveX(endValue, moveDuration)
            .SetEase(Ease.Linear);
    }
    private void MoveRight()
    {
        if (_curentValueX >= radius) return;

        ServiceLocator.GetService<AudioManager>().PlayTargetAudio();

        float endValue = _curentValueX + radius;
        _curentValueX = endValue;
        transform.DOKill();
        transform.DOMoveX(endValue, moveDuration)
            .SetEase(Ease.Linear);
    }

    private void OnTriggerEnter(Collider other)
    {
        ITakeDamage takeDamage = other.GetComponent<ITakeDamage>();
        _targetIndex = takeDamage.index;
        _targetColl = other; 
        if (takeDamage != null) onTargetEnter?.Invoke(takeDamage);    
    }
    private void OnTriggerExit(Collider other)
    {
        onTargetExit?.Invoke(); 
    }

    private void AttackState(ITakeDamage takeDamage)
    {
        if (_targetColl == null || !_targetColl.enabled)
        {
            onTargetExit?.Invoke(); 
            return;
        }

        _icon.transform.DOKill();
        _mat.DOKill(); 

        float endValue = _icon.transform.localScale.x > def_size ?  def_size : big_size; 
        Vector3 size = new Vector3(endValue, endValue, endValue);
        _icon.transform.DOScale(size, tween_duration * .3f).OnComplete(() => AttackState(takeDamage));

        Color color = _indexInfo.GetIndexColor(_targetIndex); /*_mat.color == Color.white ? Color.red : Color.white*/; 
        _mat.DOColor(color, tween_duration * .3f);
    }
    private void IdleState()
    {
        _icon.transform.DOKill();
        _mat.DOKill();

        Vector3 dir = new Vector3(90, _icon.transform.eulerAngles.y + 90, 0);
        _icon.transform.DORotate(dir, tween_duration).OnComplete(() => IdleState());

        if (_icon.transform.localScale.x != def_size)
        {
            Vector3 size = new Vector3(def_size, def_size, def_size);
            _icon.transform.DOScale(size, tween_duration * .3f);
        }
        if(_mat.color != Color.white)
            _mat.DOColor(Color.white, tween_duration * .3f);
    }
}
