using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Manages all audio playback in the game using a singleton pattern
/// IMPROVEMENT: Should implement proper singleton with null checks and DontDestroyOnLoad
/// IMPROVEMENT: Consider using UnityEvents to decouple audio from other systems
/// </summary>
public class AudioController : MonoBehaviour
{
    // AudioSource for general sound effects (clicks, stamps, etc.)
    [SerializeField] private AudioSource universal;

    // AudioSource specifically for train movement sounds
    [SerializeField] private AudioSource train;

    // Duration in seconds for train sound fade-out effect
    [SerializeField] private float fadeDuration = 1f;

    // Static singleton instance for global access
    // IMPROVEMENT: Should check if Instance already exists and destroy duplicates
    public static AudioController Instance;

    /// <summary>
    /// Initialize singleton instance on object creation
    /// IMPROVEMENT: Add null check - if (Instance != null && Instance != this) Destroy(gameObject);
    /// IMPROVEMENT: Consider DontDestroyOnLoad(gameObject) if audio persists between scenes
    /// </summary>
    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Plays a one-shot audio clip through the universal AudioSource
    /// Used for UI clicks, stamp sounds, and other short sound effects
    /// IMPROVEMENT: Add null checks for Instance and audio parameter
    /// </summary>
    public static void PlayClip(AudioClip audio)
    {
        Instance.universal.PlayOneShot(audio);
    }

    /// <summary>
    /// Starts playing the train movement sound
    /// Called when player begins traveling between stations
    /// IMPROVEMENT: Add null check for Instance
    /// </summary>
    public static void PlayTrain()
    {
        Instance.train.Play();
    }

    /// <summary>
    /// Stops train sound with a smooth fade-out effect
    /// Called when player reaches destination station
    /// IMPROVEMENT: Add null check for Instance before starting coroutine
    /// </summary>
    public static void StopTrain()
    {
        Instance.StartCoroutine(Instance.FadeTrain());
    }

    /// <summary>
    /// Coroutine that gradually fades out train audio over fadeDuration
    /// Prevents abrupt audio cutoff for better user experience
    /// IMPROVEMENT: Consider using DOTween or similar for smoother fade curves
    /// </summary>
    IEnumerator FadeTrain()
    {
        // Store original volume to restore after stopping
        float startVolume = train.volume;

        // Gradually reduce volume to 0
        while (train.volume > 0f)
        {
            train.volume -= Time.deltaTime / fadeDuration;
            yield return null; // Wait one frame before continuing
        }

        // Stop playback and restore volume for next use
        train.Stop();
        train.volume = startVolume;
    }

}
