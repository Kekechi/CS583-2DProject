using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField] private AudioSource universal;
    [SerializeField] private AudioSource train;
    [SerializeField] private float fadeDuration = 1f;
    public static AudioController Instance;

    void Awake()
    {
        Instance = this;
    }

    public static void PlayClip(AudioClip audio)
    {
        Instance.universal.PlayOneShot(audio);
    }

    public static void PlayTrain()
    {
        Instance.train.Play();
    }

    public static void StopTrain()
    {
        Instance.StartCoroutine(Instance.FadeTrain());
    }

    IEnumerator FadeTrain()
    {
        float startVolume = train.volume;
        while (train.volume > 0f)
        {
            train.volume -= Time.deltaTime / fadeDuration;
            yield return null;
        }

        train.Stop();
        train.volume = startVolume;
    }

}
