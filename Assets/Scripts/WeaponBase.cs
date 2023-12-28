using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [SerializeField] protected float _damageValue;
    [SerializeField] protected int _damgeIndex;
    [SerializeField] protected float _shootDelay;
    [Space(5)]
    [SerializeField] private ParticleSystem _shootEffect;
    [SerializeField] private MeshRenderer _iconMR;
    [SerializeField] private IndexInfo _indexInfo;
    [SerializeField] private AudioClip _audioEffect;  

    private TargetController _targetController;
    private ITakeDamage _takeDamage;

    protected virtual void Start()
    {
        Construct(); 
    }
    protected virtual void Construct()
    {
        _targetController = ServiceLocator.GetService<TargetController>();
        _targetController.onTargetEnter += OnEnemyEnterTarget;
        _targetController.onTargetExit += OnEnemyExitTarget;

        _iconMR.material.color = _indexInfo.GetIndexColor(_damgeIndex); 
    }
    protected virtual void Update()
    {
        LookTarget(); 
    }
    private void LookTarget()
    {
        transform.LookAt(_targetController.transform.position); 
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        _shootEffect.transform.LookAt(_targetController.transform.position + Vector3.up * 5);
    }
    private void OnEnemyEnterTarget(ITakeDamage takeDamage)
    {
        _takeDamage = takeDamage; 
    }
    private void OnEnemyExitTarget()
    {
        _takeDamage = null; 
    }
    protected virtual void Shoot()
    {
        if (_takeDamage == null) return;

        ServiceLocator.GetService<AudioManager>().PlayOneShot(_audioEffect); 
        _shootEffect.Play();
        _takeDamage.TakeDamage(_damageValue, _damgeIndex);

    }
    private void OnMouseDown()
    {
        Shoot();
    }
}
