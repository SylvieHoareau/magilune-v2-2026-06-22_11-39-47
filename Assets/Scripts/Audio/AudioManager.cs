using UnityEngine;

/// <summary>
/// Gestion centrale de l'audio (SFX + musique).
/// Singleton DontDestroyOnLoad.
/// Compatible avec : AudioManager.Instance.PlaySFX(clip)
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Volumes (0 → 1)")]
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.6f;

    [Header("Options")]
    [SerializeField] private bool musicLoops = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetupSources();
        ApplyVolumes();
    }

    private void SetupSources()
    {
        // Crée les AudioSource si elles n'existent pas
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f; // 2D
        }

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = musicLoops;
            musicSource.spatialBlend = 0f;
        }
        else
        {
            musicSource.loop = musicLoops;
        }
    }

    private void ApplyVolumes()
    {
        if (sfxSource != null) sfxSource.volume = sfxVolume;
        if (musicSource != null) musicSource.volume = musicVolume;
    }

    // -------------------------------------------------------------------------
    // SFX
    // -------------------------------------------------------------------------

    /// <summary>Joue un effet sonore (one-shot).</summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    /// <summary>Joue un SFX avec un volume personnalisé (0 → 1).</summary>
    public void PlaySFX(AudioClip clip, float volumeScale)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume * Mathf.Clamp01(volumeScale));
    }

    // -------------------------------------------------------------------------
    // Musique
    // -------------------------------------------------------------------------

    public void PlayMusic(AudioClip music)
    {
        if (music == null || musicSource == null) return;

        // Déjà en train de jouer ce clip → ne rien faire
        if (musicSource.clip == music && musicSource.isPlaying) return;

        musicSource.clip = music;
        musicSource.volume = musicVolume;
        musicSource.loop = musicLoops;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
            musicSource.Stop();
    }

    public void PauseMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
            musicSource.Pause();
    }

    public void ResumeMusic()
    {
        if (musicSource != null && !musicSource.isPlaying && musicSource.clip != null)
            musicSource.UnPause();
    }

    // -------------------------------------------------------------------------
    // Volumes runtime (pour un futur menu Options)
    // -------------------------------------------------------------------------

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null) sfxSource.volume = sfxVolume;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null) musicSource.volume = musicVolume;
    }

    public float GetSFXVolume() => sfxVolume;
    public float GetMusicVolume() => musicVolume;
}
