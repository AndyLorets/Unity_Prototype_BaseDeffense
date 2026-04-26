using UnityEngine;

public class ShootButton : MonoBehaviour
{
    [SerializeField] private WeaponTurel[] _turrets;

    // Assign to UI Button → OnClick()
    public void OnPress()
    {
        foreach (var turret in _turrets)
            turret.ManualShoot();
    }
}
