using System.Collections;
using UnityEngine;

public class WeaponTurel : WeaponBase
{
    protected override void Start()
    {
        base.Start();
        StartCoroutine(AutoShootLoop());
    }

    private IEnumerator AutoShootLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(_shootDelay);
            if (CurrentTarget != null)
                Shoot();
        }
    }
}
