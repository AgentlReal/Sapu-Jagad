using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    public AudioClip menuMusic;
    public AudioClip gameplayMusic;

    private AudioSource musicSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (musicSource != null)
        {
            musicSource.volume = AudioSettings.MasterVolume;
        }
    }

    public void PlayMenuMusic()
    {
        if (musicSource.clip == menuMusic && musicSource.isPlaying) return;
        musicSource.clip = menuMusic;
        musicSource.Play();
    }

    public void PlayGameplayMusic()
    {
        if (musicSource.clip == gameplayMusic && musicSource.isPlaying) return;
        musicSource.clip = gameplayMusic;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }
}
