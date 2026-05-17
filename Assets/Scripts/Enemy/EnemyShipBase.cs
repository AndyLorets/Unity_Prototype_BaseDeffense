using DG.Tweening;
using UnityEngine;

public abstract class EnemyShipBase : MonoBehaviour, ITakeDamage
{
    public int index { get; private set; }
    public float moveSpeed { get; private set; }

    public const float MaxHp = 100f;

    private float _hp = MaxHp;
    private bool _isDead;

    public float CurrentHp => _hp;
    public bool IsDead => _isDead;

    // death flight state
    private float _deathFallSpeed;
    private float _deathRoll;
    private float _deathRollSpeed;
    private float _deathYaw;
    private float _deathYawSpeed;

    protected Collider _coll;

    [SerializeField] private IndexInfo _indexInfo;
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private ParticleSystem _deathEffect;
    [SerializeField] private Vector3 _healthBarOffset = new Vector3(0f, 2f, 0f);

    private Vector3 _deathEffectPos;
    private Quaternion _startRotation;
    private HealthBarUI _healthBar;
    public System.Action<EnemyShipBase> onDeath;

    private void Awake()
    {
        _coll = GetComponent<Collider>();
        _startRotation = transform.localRotation;
    }

    protected virtual void Update()
    {
        if (_isDead)
        {
            DeathFlight();
        }

        Move();
    }

    private void Move()
    {
        float speed = _isDead ? moveSpeed * 2 : moveSpeed; 
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    private void DeathFlight()
    {
        float dt = Time.deltaTime;

        _deathFallSpeed += 15f * dt; // гравитация
        transform.Translate(Vector3.down * _deathFallSpeed * dt, Space.World);

        // крен и рыскание плавно меняются
        _deathRoll += _deathRollSpeed * dt;
        _deathYaw  += _deathYawSpeed  * dt;

        transform.localRotation = Quaternion.Euler(0f, _deathYaw, _deathRoll);
    }

    private void OnEnable()
    {
        AcquireHealthBar();

        if (EnemySpawnManager.Instance != null)
            EnemySpawnManager.Instance.RegisterActive(this);
    }

    private void OnDisable()
    {
        ReleaseHealthBar();

        if (EnemySpawnManager.Instance != null)
            EnemySpawnManager.Instance.UnregisterActive(this);
    }

    private void AcquireHealthBar()
    {
        if (_healthBar != null) return;
        HealthBarPool pool = ServiceLocator.GetService<HealthBarPool>();
        if (pool == null) return;
        _healthBar = pool.Get(transform, _healthBarOffset);
        _healthBar.SetHealthInstant(_hp, MaxHp);
    }

    private void ReleaseHealthBar()
    {
        if (_healthBar == null) return;
        ServiceLocator.GetService<HealthBarPool>().Release(_healthBar);
        _healthBar = null;
    }

    public void TakeDamage(float value, int index)
    {
        if (_isDead) return;

        if (index != -1)
            value = index == this.index ? value : value * .2f;

        ApplyDamage(value);
    }

    public void TakeDamagePercent(float percent01)
    {
        if (_isDead) return;
        if (percent01 <= 0f) return;

        ApplyDamage(percent01 * MaxHp);
    }

    private void ApplyDamage(float value)
    {
        _hp -= value;

        if (_hp <= 0f)
            _hp = 0f;

        if (_healthBar != null)
            _healthBar.UpdateHealth(_hp, MaxHp);

        if (_hp <= 0f && !_isDead)
        {
            _isDead = true;
            Death();
        }
    }

    protected virtual void Death()
    {
        transform.DOKill();

        _coll.enabled = false;
        _deathEffect.transform.parent = null;
        _deathEffect.Play();
        ServiceLocator.GetService<AudioManager>().PlayExplosionAudio();

        _deathFallSpeed = 0f;
        _deathRoll = transform.localEulerAngles.z;
        _deathYaw  = transform.localEulerAngles.y;
        _deathRollSpeed = Random.Range(30f, 40f) * -1f; 
        _deathYawSpeed  = Random.Range(20f, 40f) * (Random.value > 0.5f ? 1f : -1f);

        // схлопнуться незадолго до Reconstruct
        transform.DOScale(Vector3.zero, 3f).SetDelay(.5f).SetEase(Ease.InBack);

        ReleaseHealthBar();

        if (EnemySpawnManager.Instance != null)
            EnemySpawnManager.Instance.UnregisterActive(this);

        onDeath?.Invoke(this);
        ScoreManager.AddScore(5);
        Invoke(nameof(Reconstruct), 3f);
    }

    public void Construct(float movespeed, int inx)
    {
        index = inx;
        moveSpeed = movespeed;
        _meshRenderer.material.color = _indexInfo.GetIndexColor(inx);
        _deathEffectPos = _deathEffect.transform.position;
    }

    private void Reconstruct()
    {
        transform.DOKill();
        transform.localScale = Vector3.one;
        transform.localRotation = _startRotation;

        gameObject.SetActive(false);

        _hp = MaxHp;
        _isDead = false;
        _coll.enabled = true;
        _deathEffect.transform.parent = transform;
        _deathEffect.transform.position = _deathEffectPos;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(Tags.Basa))
        {
            ScoreManager.RemoveScore(10);
            Reconstruct();
        }
    }
}
