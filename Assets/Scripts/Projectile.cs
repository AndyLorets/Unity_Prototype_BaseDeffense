using UnityEngine;

public class Projectile : MonoBehaviour
{
    public enum MoveMode { Straight, HomingTarget }

    [Header("Movement")]
    [SerializeField] private MoveMode _moveMode = MoveMode.Straight;
    [SerializeField] private float _speed = 60f;
    [SerializeField] private float _maxLifetime = 4f;
    [SerializeField] private float _arrivalDistance = 0.3f;

    [Header("Visual / Audio")]
    [SerializeField] private ParticleSystem _impactEffect;
    [SerializeField] private GameObject _visualRoot;
    [SerializeField] private AudioClip _impactAudio;

    private Vector3 _targetPoint;
    private Transform _homingTarget;
    private bool _launched;
    private float _spawnTime;

    public void Launch(Vector3 targetPoint, Transform homingTarget = null)
    {
        _targetPoint = targetPoint;
        _homingTarget = homingTarget;
        _launched = true;
        _spawnTime = Time.time;

        if (_visualRoot != null) _visualRoot.SetActive(true);

        FaceTarget();
    }

    private void Update()
    {
        if (!_launched) return;

        if (Time.time - _spawnTime > _maxLifetime)
        {
            Despawn();
            return;
        }

        Vector3 target = ResolveTargetPoint();
        Vector3 toTarget = target - transform.position;
        float dist = toTarget.magnitude;

        if (dist <= _arrivalDistance || dist <= _speed * Time.deltaTime)
        {
            transform.position = target;
            PlayImpactFx(target);
            Despawn();
            return;
        }

        transform.position += toTarget.normalized * _speed * Time.deltaTime;
        FaceTarget();
    }

    private Vector3 ResolveTargetPoint()
    {
        if (_moveMode == MoveMode.HomingTarget && _homingTarget != null)
            return _homingTarget.position;
        return _targetPoint;
    }

    private void FaceTarget()
    {
        Vector3 dir = ResolveTargetPoint() - transform.position;
        if (dir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    private void PlayImpactFx(Vector3 at)
    {
        if (_impactEffect != null)
        {
            _impactEffect.transform.parent = null;
            _impactEffect.transform.position = at;
            _impactEffect.Play();
        }

        if (_impactAudio != null)
            ServiceLocator.GetService<AudioManager>()?.PlayOneShot(_impactAudio);
    }

    private void Despawn()
    {
        _launched = false;
        Destroy(gameObject);
    }
}
