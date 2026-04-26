using System.Collections;
using UnityEngine;

public class WeaponTurel : WeaponBase
{
    public enum ShootMode { Auto, Button }

    [SerializeField] private ShootMode _shootMode;
    [SerializeField] private float _searchRadius = 20f;

    private bool _isReloading;

    protected override void Start()
    {
        base.Start();
        if (_shootMode == ShootMode.Auto)
            StartCoroutine(AutoShootLoop());
    }

    // Called by UI Button
    public void ManualShoot()
    {
        if (_shootMode != ShootMode.Button || _isReloading) return;
        StartCoroutine(Reload());
    }

    private IEnumerator AutoShootLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(_shootDelay);
            Shoot();
        }
    }

    private IEnumerator Reload()
    {
        _isReloading = true;
        FireButton();
        yield return new WaitForSeconds(_shootDelay);
        _isReloading = false;
    }

    // Button mode: fires at crosshair target first, then searches nearby
    private void FireButton()
    {
        ITakeDamage target = CurrentTarget;
        if (target != null)
        {
            ShootAt(target);
            return;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, _searchRadius);
        float closest = float.MaxValue;
        ITakeDamage nearestTarget = null;

        foreach (var hit in hits)
        {
            ITakeDamage t = hit.GetComponent<ITakeDamage>();
            if (t == null) continue;
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < closest)
            {
                closest = dist;
                nearestTarget = t;
            }
        }

        ShootAt(nearestTarget);
    }
}
