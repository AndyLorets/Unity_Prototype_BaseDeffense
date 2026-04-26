using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [SerializeField] protected float _damageValue;
    [SerializeField] protected int _damgeIndex;
    [SerializeField] protected float _shootDelay;
    [SerializeField] protected bool _universalDamage;
    [Space(5)]
    [SerializeField] private ParticleSystem _shootEffect;
    [SerializeField] private MeshRenderer _iconMR;
    [SerializeField] private IndexInfo _indexInfo;
    [SerializeField] private AudioClip _audioEffect;
    [SerializeField] private CrosshairController _crosshair;

    protected ITakeDamage CurrentTarget => _crosshair != null ? _crosshair.CurrentTarget : null;

    protected virtual void Start()
    {
        _iconMR.material.color = _indexInfo.GetIndexColor(_damgeIndex);
    }

    protected virtual void Update()
    {
        if (_crosshair != null)
        {
            transform.LookAt(_crosshair.transform.position);
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
            _shootEffect.transform.LookAt(_crosshair.transform.position + Vector3.up * 5);
        }
    }

    protected virtual void Shoot()
    {
        ITakeDamage target = CurrentTarget;
        if (target == null) return;

        ServiceLocator.GetService<AudioManager>().PlayOneShot(_audioEffect);
        _shootEffect.Play();
        int effectiveIndex = _universalDamage ? -1 : _damgeIndex;
        target.TakeDamage(_damageValue, effectiveIndex);
    }

    protected void ShootAt(ITakeDamage target)
    {
        ServiceLocator.GetService<AudioManager>().PlayOneShot(_audioEffect);
        _shootEffect.Play();

        if (target == null) return;

        int effectiveIndex = _universalDamage ? -1 : _damgeIndex;
        target.TakeDamage(_damageValue, effectiveIndex);
    }
}
