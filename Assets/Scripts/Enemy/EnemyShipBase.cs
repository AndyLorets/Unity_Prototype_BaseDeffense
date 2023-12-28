using DG.Tweening;
using UnityEngine;

public abstract class EnemyShipBase : MonoBehaviour, ITakeDamage
{
    public int index { get; private set; }
    public float moveSpeed { get; private set; }

    private float _hp = 100f;
    private bool _isDead;

    protected Rigidbody _rb;
    protected Collider _coll;

    [SerializeField] private IndexInfo _indexInfo;
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private ParticleSystem _deathEffect;

    private Vector3 _deathEffectPos; 
    public System.Action<EnemyShipBase> onDeath; 

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _coll = GetComponent<Collider>();
    }
    protected virtual void Update()
    {
        if (_isDead) return;

        Move(); 
    }
    private void Move()
    {
        transform.Translate(Vector3.right * moveSpeed * Time.deltaTime); 
    }
    public void TakeDamage(float value, int index)
    {
        value = index == this.index ? value : value * .2f;
        _hp -= value;

        if (_hp <= 0) 
            Invoke(nameof(Death), .5f);
    }

    protected virtual void Death()
    { 
        Vector3 rh = transform.localPosition.x < 0 ? Vector3.right : Vector3.left; 
        _rb.constraints = RigidbodyConstraints.None;
        _rb.AddForceAtPosition((Vector3.up * 26) + (rh * 45), Vector3.zero, ForceMode.Impulse);
        _rb.AddTorque(Vector3.up + rh * 24, ForceMode.Impulse); 

        _coll.enabled = false;
        _deathEffect.transform.parent = null;

        _deathEffect.Play();
        ServiceLocator.GetService<AudioManager>().PlayExplosionAudio();

        _isDead = true;

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
        gameObject.SetActive(false);

        _hp = 100f;
        _rb.constraints = RigidbodyConstraints.FreezeAll;
        _coll.enabled = true;
        _deathEffect.transform.parent = transform;
        _deathEffect.transform.position = _deathEffectPos; 
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag(Tags.Basa))
        {
            ScoreManager.RemoveScore(10);
            Reconstruct();
        }
    }
}
