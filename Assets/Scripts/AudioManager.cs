using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] public AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip background;
    public AudioClip gunShot;
    public AudioClip reload;
    public AudioClip grapple;
    public AudioClip button;
    public AudioClip machineGun;
    public AudioClip explosion;
    public AudioClip laserBeam;
    public AudioClip enemyFire;
    public AudioClip enemyDeath;
    public AudioClip levelChanger;
    public AudioClip doorClose;

    [Header("Scenes Without Music")]
    [SerializeField] private string[] scenesWithoutMusic; // Add scene names in Inspector

    private static AudioManager instance; // Singleton

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        if (musicSource == null) return;
      
        musicSource.clip = background;
        musicSource.loop = true;
        musicSource.Play();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (musicSource == null)
        {
            return;
        }

        // Check if scene is in the list
        bool shouldStopMusic = System.Array.Exists(scenesWithoutMusic, sceneName => sceneName == scene.name);

        if (shouldStopMusic)
        {
            musicSource.volume = 0;
            musicSource.Stop();
        }
        else
        {
            // Reset volume to 1 when entering a scene that should have music
            musicSource.volume = 1f;

            if (!musicSource.isPlaying)
            {
                musicSource.Play();
            }
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null)
            sfxSource.PlayOneShot(clip);
    }
}
