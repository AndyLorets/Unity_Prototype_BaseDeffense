using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public enum AttackKind { NearestToBase, CrosshairRadius }

public class WeaponSpecial : WeaponBase
{
    [Header("Refs")]
    [SerializeField] private Transform _basePoint;
    [SerializeField] private CrosshairController _cursorCrosshair;
    [SerializeField] private Transform _aimPivot;
    [SerializeField] private Transform _muzzle;

    [Header("Projectile (visual)")]
    [SerializeField] private Projectile _projectilePrefab;

    [Header("Nearest-to-base attack")]
    [Range(0f, 1f)]
    [SerializeField] private float _nearestDamagePercent = 0.2f;
    [SerializeField] private float _nearestCooldown = 1f;
    [SerializeField] private Vector3 _activationZoneCenter = Vector3.zero;
    [SerializeField] private Vector3 _activationZoneSize = new Vector3(20f, 0f, 10f);

    [Header("Cursor radius attack")]
    [SerializeField] private float _splashRadius = 30f;
    [SerializeField] private float _directHitRadius = 12f;
    [Range(0f, 1f)]
    [SerializeField] private float _radiusFullDamagePercent = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float _radiusSplashDamagePercent = 0.5f;
    [SerializeField] private float _radiusCooldown = 5f;

    [Header("Aim tween (CursorRadius)")]
    [SerializeField] private float _aimDuration = 0.15f;
    [SerializeField] private float _holdAfterShot = 0.5f;
    [SerializeField] private float _returnDuration = 0.25f;
    [SerializeField] private Ease _aimEase = Ease.OutQuad;
    [SerializeField] private Ease _returnEase = Ease.InOutQuad;

    public float NearestCooldown => _nearestCooldown;
    public float RadiusCooldown => _radiusCooldown;

    private float _nearestNextReadyTime;
    private float _radiusNextReadyTime;

    private Quaternion _aimRestWorldRotation;
    private bool _aimRestCaptured;
    private Tween _aimTween;

    private readonly List<EnemyShipBase> _aoeBuffer = new List<EnemyShipBase>(32);

    private Transform AimPivot => _aimPivot != null ? _aimPivot : transform;
    private Vector3 MuzzlePosition => _muzzle != null ? _muzzle.position : transform.position;

    private void Awake()
    {
        CaptureAimRest();
    }

    private void CaptureAimRest()
    {
        if (_aimRestCaptured) return;
        _aimRestWorldRotation = AimPivot.rotation;
        _aimRestCaptured = true;
    }

    public bool IsReady(AttackKind kind)
    {
        float now = Time.time;
        if (kind == AttackKind.NearestToBase)
            return now >= _nearestNextReadyTime && FindNearestInActivationZone() != null;
        return now >= _radiusNextReadyTime;
    }

    public float GetCooldownRemaining(AttackKind kind)
    {
        float now = Time.time;
        float remaining = kind == AttackKind.NearestToBase
            ? _nearestNextReadyTime - now
            : _radiusNextReadyTime - now;
        return Mathf.Max(0f, remaining);
    }

    public bool TryFireNearestToBase()
    {
        if (Time.time < _nearestNextReadyTime) return false;
        if (_basePoint == null) return false;

        EnemyShipBase target = FindNearestInActivationZone();
        if (target == null) return false;

        _nearestNextReadyTime = Time.time + _nearestCooldown;

        PlayShootFx();
        target.TakeDamagePercent(_nearestDamagePercent);
        SpawnVisualProjectile(target.transform.position, target.transform);

        return true;
    }

    public EnemyShipBase FindNearestInActivationZone()
    {
        EnemySpawnManager spawn = EnemySpawnManager.Instance;
        if (spawn == null || _basePoint == null) return null;

        Vector3 halfSize = _activationZoneSize * 0.5f;

        EnemyShipBase nearest = null;
        float minSqr = float.MaxValue;
        Vector3 basePos = _basePoint.position;

        IReadOnlyList<EnemyShipBase> active = spawn.ActiveEnemies;
        for (int i = 0; i < active.Count; i++)
        {
            EnemyShipBase e = active[i];
            if (e == null || e.IsDead) continue;

            Vector3 p = e.transform.position;
            Vector3 local = _basePoint.InverseTransformPoint(p) - _activationZoneCenter;
            if (Mathf.Abs(local.x) > halfSize.x || Mathf.Abs(local.z) > halfSize.z) continue;

            Vector3 d = p - basePos;
            d.y = 0f;
            float sqr = d.sqrMagnitude;
            if (sqr < minSqr)
            {
                minSqr = sqr;
                nearest = e;
            }
        }
        return nearest;
    }

    public bool TryFireAtCursorRadius()
    {
        if (!IsReady(AttackKind.CrosshairRadius)) return false;
        if (_cursorCrosshair == null) return false;
        if (EnemySpawnManager.Instance == null) return false;

        _radiusNextReadyTime = Time.time + _radiusCooldown;
        StartAimAndFireTween(_cursorCrosshair.transform.position);
        return true;
    }

    private void StartAimAndFireTween(Vector3 cursorWorldPos)
    {
        CaptureAimRest();
        _aimTween?.Kill();

        Transform pivot = AimPivot;
        Vector3 flatDir = cursorWorldPos - pivot.position;
        flatDir.y = 0f;
        if (flatDir.sqrMagnitude < 0.0001f)
        {
            FireRadiusAt(cursorWorldPos);
            return;
        }

        Quaternion targetWorldRot = Quaternion.LookRotation(flatDir, Vector3.up);

        Sequence seq = DOTween.Sequence();
        seq.Append(pivot.DORotateQuaternion(targetWorldRot, _aimDuration).SetEase(_aimEase));
        seq.AppendCallback(() => FireRadiusAt(cursorWorldPos));
        if (_holdAfterShot > 0f)
            seq.AppendInterval(_holdAfterShot);
        seq.Append(pivot.DORotateQuaternion(_aimRestWorldRotation, _returnDuration).SetEase(_returnEase));
        _aimTween = seq;
    }

    private void FireRadiusAt(Vector3 center)
    {
        PlayShootFx();

        // Если прицел захватил врага (красный) — гарантированно бьём по нему.
        // Splash вокруг — по позиции этого врага, чтобы AoE задело соседей.
        EnemyShipBase locked = _cursorCrosshair != null
            ? _cursorCrosshair.CurrentTarget as EnemyShipBase
            : null;

        Vector3 impactCenter;
        Transform visualHoming;

        if (locked != null && !locked.IsDead)
        {
            locked.TakeDamagePercent(_radiusFullDamagePercent);
            impactCenter = locked.transform.position;
            visualHoming = locked.transform;
            ApplySplashAround(impactCenter, locked);
        }
        else
        {
            impactCenter = center;
            visualHoming = null;
            ApplyAreaDamage(center);
        }

        SpawnVisualProjectile(impactCenter, visualHoming);
    }

    private void ApplyAreaDamage(Vector3 center)
    {
        EnemySpawnManager spawn = EnemySpawnManager.Instance;
        if (spawn == null) return;

        spawn.FindEnemiesInRadius(center, _splashRadius, _aoeBuffer);
        float directSqr = _directHitRadius * _directHitRadius;

        for (int i = 0; i < _aoeBuffer.Count; i++)
        {
            EnemyShipBase enemy = _aoeBuffer[i];
            if (enemy == null || enemy.IsDead) continue;

            Vector3 d = enemy.transform.position - center;
            d.y = 0f;
            float pct = d.sqrMagnitude <= directSqr
                ? _radiusFullDamagePercent
                : _radiusSplashDamagePercent;
            enemy.TakeDamagePercent(pct);
        }
    }

    private void ApplySplashAround(Vector3 center, EnemyShipBase exclude)
    {
        EnemySpawnManager spawn = EnemySpawnManager.Instance;
        if (spawn == null) return;

        spawn.FindEnemiesInRadius(center, _splashRadius, _aoeBuffer);

        for (int i = 0; i < _aoeBuffer.Count; i++)
        {
            EnemyShipBase enemy = _aoeBuffer[i];
            if (enemy == null || enemy.IsDead || enemy == exclude) continue;
            enemy.TakeDamagePercent(_radiusSplashDamagePercent);
        }
    }

    private void SpawnVisualProjectile(Vector3 targetPoint, Transform homingTarget)
    {
        if (_projectilePrefab == null) return;
        Projectile p = Instantiate(_projectilePrefab, MuzzlePosition, Quaternion.identity);
        p.Launch(targetPoint, homingTarget);
    }

    private void OnDestroy()
    {
        _aimTween?.Kill();
    }

    private void OnDrawGizmosSelected()
    {
        if (_cursorCrosshair != null)
        {
            Vector3 c = _cursorCrosshair.transform.position;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(c, _directHitRadius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(c, _splashRadius);
        }

        if (_basePoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_basePoint.position, 0.5f);
        }

        if (_muzzle != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(_muzzle.position, 0.2f);
        }

        if (_basePoint != null)
        {
            Matrix4x4 prev = Gizmos.matrix;
            Gizmos.matrix = _basePoint.localToWorldMatrix;

            Vector3 gizmoSize = new Vector3(_activationZoneSize.x, Mathf.Max(_activationZoneSize.y, 2f), _activationZoneSize.z);
            Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
            Gizmos.DrawCube(_activationZoneCenter, gizmoSize);
            Gizmos.color = new Color(0f, 1f, 0f, 1f);
            Gizmos.DrawWireCube(_activationZoneCenter, gizmoSize);

            Gizmos.matrix = prev;
        }
    }
}
