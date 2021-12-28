using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManagerBehavior : MonoBehaviour
{
    public static GameObject instance;
    
    #region Fields
    /// <summary>
    /// The first AudioClip in this array is the title theme.
    /// </summary>
    public AudioClip[] musicClips;
    public AudioSource musicSource, sfxSource;

    public GameBoardBehavior gb;
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
        }

        if (musicSource == null || sfxSource == null)
        {
            throw new UnassignedReferenceException(string.Format("Please make sure that {0} and {1} fields are assigned.", nameof(musicSource), nameof(sfxSource)));
        }

        SceneManager.OnSceneLoad += PlayNewSong;
    }
    #endregion

    #region Methods
    public static void RegisterGamePiece(PieceBehavior pieceToRegister)
    {
        pieceToRegister.OnBreak += PlayPopNoise;
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

    private static void PlayPopNoise()
    {
        SoundManagerBehavior instBehavior = instance.GetComponent<SoundManagerBehavior>();
        instBehavior.sfxSource.Play();
    }
    #endregion
}
