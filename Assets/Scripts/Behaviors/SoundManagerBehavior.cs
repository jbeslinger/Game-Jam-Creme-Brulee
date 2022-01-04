using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManagerBehavior : MonoBehaviour
{
    public static GameObject instance;

    #region Fields
    [Header("Audio Clips")]
    /// <summary>
    /// The first AudioClip in this array is the title theme.
    /// </summary>
    public AudioClip[] musicClips;
    public AudioClip breakSound;
    public AudioClip winSound;
    public AudioClip loseSound;

    [Header("Sources")]
    public AudioSource musicSource, sfxSource;
    #endregion

    #region Members
    private GameBoardBehavior _gb;
    private bool _eventsAttached = false;
    #endregion

    #region Unity Methods
    private void OnEnable()
    {
        if (instance == null)
        {
            instance = this.gameObject;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            DestroyImmediate(this.gameObject);
            return;
        }

        if (musicSource == null || sfxSource == null)
        {
            throw new UnassignedReferenceException(string.Format("Please make sure that {0} and {1} fields are assigned.", nameof(musicSource), nameof(sfxSource)));
        }

        if (winSound == null || loseSound == null)
        {
            Debug.LogWarning(string.Format("You have not assigned either {0}, {1}, or both so nothing will play.", nameof(winSound), nameof(loseSound)));
        }

        SceneManager.OnSceneLoad += () =>
        {
            if (_gb != null)
            {
                DetachEvents();
                _gb = null;
            }

            PlayNewSong();
            StartCoroutine(FadeIn(musicSource, 0.1f, 0.25f));
        };
    }

    private void Update()
    {
        if (_gb == null)
        {
            ScanForGameBoard();
        }
        else
        {
            if (!_eventsAttached)
            {
                AttachEvents();
            }
        }
    }
    #endregion

    #region Methods
    public static void RegisterGamePiece(PieceBehavior pieceToRegister)
    {
        pieceToRegister.OnBreak += PlayBreakSound;
    }

    private void ScanForGameBoard()
    {
        _gb = FindObjectOfType<GameBoardBehavior>();
    }

    private void AttachEvents()
    {
        _eventsAttached = true;
        _gb.OnWinConditionMet += () =>
        {
            PlayWinSound();
            StartCoroutine(FadeOut(musicSource, 2.0f));
        };

        _gb.OnGameOver += () =>
        {
            PlayLoseSound();
            StartCoroutine(FadeOut(musicSource, 2.0f));
        };
    }

    private void DetachEvents()
    {
        _eventsAttached = false;
        _gb.OnWinConditionMet -= () => { StartCoroutine(FadeOut(musicSource, 1.0f)); };
        _gb.OnGameOver -= () => { StartCoroutine(FadeOut(musicSource, 1.0f)); };
    }

    private static void PlayNewSong()
    {
        SoundManagerBehavior instBehavior = instance.GetComponent<SoundManagerBehavior>();
        instBehavior.musicSource.Stop();
        if (instBehavior.musicClips.Length > 1)
        {
            instBehavior.musicSource.clip = instBehavior.musicClips[Random.Range(0, instBehavior.musicClips.Length)];
        }
        instBehavior.musicSource.Play();
    }

    private static void PlayBreakSound()
    {
        SoundManagerBehavior instBehavior = instance.GetComponent<SoundManagerBehavior>();
        instBehavior.sfxSource.clip = instBehavior.breakSound;
        instBehavior.sfxSource.Play();
    }

    private static void PlayWinSound()
    {
        SoundManagerBehavior instBehavior = instance.GetComponent<SoundManagerBehavior>();
        instBehavior.sfxSource.clip = instBehavior.winSound;
        instBehavior.sfxSource.Play();
    }

    private static void PlayLoseSound()
    {
        SoundManagerBehavior instBehavior = instance.GetComponent<SoundManagerBehavior>();
        instBehavior.sfxSource.clip = instBehavior.loseSound;
        instBehavior.sfxSource.Play();
    }

    public static IEnumerator FadeOut(AudioSource audioSource, float FadeTime)
    {
        float startVolume = audioSource.volume;
        while (audioSource.volume > 0f)
        {
            audioSource.volume -= startVolume * Time.deltaTime / FadeTime;

            yield return null;
        }
        audioSource.Stop();
        audioSource.volume = startVolume;
    }

    public static IEnumerator FadeIn(AudioSource audioSource, float FadeTime, float targetVolume)
    {
        float startVolume = 0.01f;
        audioSource.volume = 0f;
        while (audioSource.volume < targetVolume)
        {
            audioSource.volume += startVolume * Time.deltaTime / FadeTime;

            yield return null;
        }
        audioSource.volume = targetVolume;
    }
    #endregion
}
