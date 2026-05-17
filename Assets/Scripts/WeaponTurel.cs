using System.Collections;
using UnityEngine;

public class WeaponTurel : WeaponBase
{
    [Header("Turret")]
    [SerializeField] private bool _autoShootEnabled = true;
    [SerializeField] private float _damageValue;
    [SerializeField] private int _damgeIndex;
    [SerializeField] private float _shootDelay;
    [SerializeField] private bool _universalDamage;
    [SerializeField] private CrosshairController _crosshair;

    public bool AutoShootEnabled
    {
        get => _autoShootEnabled;
        set => _autoShootEnabled = value;
    }

    private ITakeDamage CurrentTarget => _crosshair != null ? _crosshair.CurrentTarget : null;

    protected override void Start()
    {
        _iconColorIndex = _damgeIndex;
        base.Start();
        StartCoroutine(AutoShootLoop());
    }

    private void Update()
    {
        if (_crosshair == null) return;

        transform.LookAt(_crosshair.transform.position);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        if (_shootEffect != null)
            _shootEffect.transform.LookAt(_crosshair.transform.position + Vector3.up * 5);
    }

    private IEnumerator AutoShootLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(_shootDelay);
            if (_autoShootEnabled && CurrentTarget != null)
                Shoot();
        }
    }

    private void Shoot()
    {
        ITakeDamage target = CurrentTarget;
        if (target == null) return;

        PlayShootFx();
        int effectiveIndex = _universalDamage ? -1 : _damgeIndex;
        target.TakeDamage(_damageValue, effectiveIndex);
    }
}
