using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public AudioSource[] audioSources = new AudioSource[10];
    public AudioProfile profile;
    int currentSourceIndex;
    
    public Dictionary<String, AudioProfile.Clip> library = new Dictionary<string, AudioProfile.Clip>();

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(gameObject);
        else instance = this;
        DontDestroyOnLoad(gameObject);
        profile.Move(library);

        
        for (int i = 0; i < audioSources.Length; ++i)
        {
            GameObject go = new GameObject();
            go.transform.parent = transform;
            audioSources[i] = go.AddComponent<AudioSource>();
        }
    }

    public AudioSource PlaySound(String clipName)
    {
        var sounds = library[clipName];
        var source = audioSources[currentSourceIndex];
        source.PlayOneShot(sounds.clips[UnityEngine.Random.Range(0, sounds.clips.Length)], sounds.volume);
        ++currentSourceIndex;
        if (currentSourceIndex >= audioSources.Length) currentSourceIndex = 0;
        return source;
    }
}
