using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioClipDesync : MonoBehaviour
{
    public AudioClip targetClip;

    void Start()
    {
        AudioSource[] allSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        List<AudioSource> matchingSources = new List<AudioSource>();

        foreach (AudioSource source in allSources)
        {
            if (source.clip == targetClip)
            {
                matchingSources.Add(source);
            }
        }

        foreach (AudioSource source in matchingSources)
        {
            source.Stop();
            source.time = UnityEngine.Random.Range(0f, targetClip.length);
            source.Play();
        }

        Debug.Log($"Desynced {matchingSources.Count} sources.");
    }
}
