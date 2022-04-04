using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "AudioProfile", menuName = "ScriptableObjects/AudioProfile", order = 1)]
public class AudioProfile : ScriptableObject
{
    [Serializable]
    public struct Clip
    {
        public AudioClip[] clips;
        public float volume;
    }
    
    [Serializable]
    public struct Pair
    {
        public String name;
        public Clip clip;
    }
    
    public List<Pair> library;

    public void Move(Dictionary<String, AudioProfile.Clip> _library)
    {
        for (int i = 0; i < library.Count; ++i) _library.Add(library[i].name, library[i].clip);
    }
}