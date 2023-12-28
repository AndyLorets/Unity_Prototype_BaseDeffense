using UnityEngine;
using System.Collections; 
public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource _gunAudioSource;
    [SerializeField] private AudioSource _targetAudioSource;
    [SerializeField] private AudioSource _explosionAudioSource;

    private bool _isDeleying; 
    private void Awake()
    {
        Initialize(); 
    }
    private void Initialize()
    {
        ServiceLocator.RegisterService(this); 
    }
    public void PlayTargetAudio()
    {
        _targetAudioSource.Play();
    }
    public void PlayExplosionAudio()
    {
        _explosionAudioSource.Play(); 
    }
    public void PlayOneShot(AudioClip audio, float delay = .2f, int count = 3)
    {
        if (_isDeleying) return;

        StartCoroutine(PlayOneShotCoroutine(audio, delay, count)); 
    }
    private IEnumerator PlayOneShotCoroutine(AudioClip audio, float delay, int count)
    {
        _isDeleying = true;
        int currentCount = count; 
        while (currentCount > 0)
        {
            _gunAudioSource.PlayOneShot(audio); 
            currentCount--;

            if (currentCount == 0) _isDeleying = false; 

            yield return new WaitForSeconds(delay); 
        }
    }
}
