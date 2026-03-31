using UnityEngine;
using System.Collections;
//will try to implement audio nvm if it doesnt work out

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sources")]
    public AudioSource sfxSource;
    public AudioSource musicSource;

    [Header("Music")]
    public AudioClip normalMusic;
    public AudioClip chaosMusic;
    public float musicVolume       = 0.5f;
    public float crossfadeTime     = 2f;

    [Header("SFX")]
    public AudioClip dropSound;
    public AudioClip hitSound;
    public AudioClip gameOverSound;
    public AudioClip chaosStinger;
    public AudioClip winSound;

    [Range(0f,1f)] public float sfxVolume = 1f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (musicSource != null && normalMusic != null)
        {
            musicSource.clip   = normalMusic;
            musicSource.loop   = true;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }
    }

    public void PlayDrop()     => PlaySFX(dropSound);
    public void PlayHit()      => PlaySFX(hitSound);
    public void PlayGameOver() => PlaySFX(gameOverSound);
    public void PlayWin()      => PlaySFX(winSound);

    public void PlayChaos()
    {
        PlaySFX(chaosStinger);
        if (chaosMusic != null)
            StartCoroutine(CrossfadeTo(chaosMusic));
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    IEnumerator CrossfadeTo(AudioClip next)
    {
        float half = crossfadeTime * 0.5f;
        float start = musicSource.volume;
        float t = 0f;

        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(start, 0f, t / half);
            yield return null;
        }

        musicSource.clip   = next;
        musicSource.volume = 0f;
        musicSource.Play();
        t = 0f;

        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(0f, musicVolume, t / half);
            yield return null;
        }

        musicSource.volume = musicVolume;
    }
}
