
using UnityEngine;

public class SoundClip 
{
    public AudioClip Clip;
    public float Volume;

    public SoundClip(AudioClip clip, float volume = 1f)
    {
        Clip = clip;
        Volume = volume;
    }
}