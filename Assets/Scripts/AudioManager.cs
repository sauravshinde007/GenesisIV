using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip background;
    public AudioClip gunShot;
    public AudioClip reload;
    public AudioClip grapple;
    public AudioClip button;
    public AudioClip door;

    private static AudioManager instance; // Singleton pattern

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Keep this object across scenes
        }
        else
        {
            Destroy(gameObject); // Prevent duplicate AudioManagers
            return;
        }
    }

    private void Start()
    {
   
   
            musicSource.clip = background;
            musicSource.loop = true;
            musicSource.Play();

    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }
}
