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
    public void PlayOneShot(AudioClip audio)
    {
        _gunAudioSource.PlayOneShot(audio);
    }
}
