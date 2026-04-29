using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "SoundData", menuName = "ScriptableObjects/SoundData", order = 1)]
public class SoundData : ScriptableObject
{
    public string id;
    public AudioClip[] audioClips;
    public AudioMixerGroup mixerGroup;

    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
    [Range(0f, 0.5f)] public float pitchVariance = 0.1f;
    [Range(0, 256)] public int priority = 128;

    public bool loop = false;
    public bool is3D = false;

    [Header("3D Sound Settings")]
    public float minDistance = 1f;
    public float maxDistance = 25f;
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
    [Range(0f, 5f)] public float spatialBlend = 0f;
    [Range(0f, 1.1f)] public float dopplerLevel = 0f;
    [Range(0f, 360f)] public float spread = 0f;
    public AnimationCurve customRolloff;
    public bool useCustomRolloff = false;


    public AudioClip GetRandomClip()
    {
        if (audioClips.Length == 0 || audioClips == null) 
            return null;

        int index = 0;
        if (audioClips.Length > 1)
            index = Random.Range(0, audioClips.Length);
        return audioClips[index];
    }
}
