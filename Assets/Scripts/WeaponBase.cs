using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [SerializeField] protected ParticleSystem _shootEffect;
    [SerializeField] protected MeshRenderer _iconMR;
    [SerializeField] protected IndexInfo _indexInfo;
    [SerializeField] protected AudioClip _audioEffect;
    [SerializeField] protected int _iconColorIndex;

    protected virtual void Start()
    {
        if (_iconMR != null && _indexInfo != null)
            _iconMR.material.color = _indexInfo.GetIndexColor(_iconColorIndex);
    }

    protected void PlayShootFx()
    {
        if (_audioEffect != null)
            ServiceLocator.GetService<AudioManager>().PlayOneShot(_audioEffect);
        if (_shootEffect != null)
            _shootEffect.Play();
    }
}
