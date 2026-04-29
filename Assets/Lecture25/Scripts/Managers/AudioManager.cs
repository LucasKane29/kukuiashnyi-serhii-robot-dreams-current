using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour, IService, ISaveable
{
    [SerializeField]
    private AudioMixer _audioMixer;
    [SerializeField]
    private int _poolSize = 20;
    [SerializeField]
    private SoundData[] _sounds;

    [SerializeField]
    private string _mainMenuSceneName;

    [SerializeField]
    private string _gameSceneName;

    private Dictionary<string, SoundData> _soundDictionary;
    private Queue<AudioSource> _audioSourcePool;
    private AudioSource _musicSource;
    private SaveSystemManager _saveSystemManager;

    private void Awake()
    {
        InitializeDictionary();
        InitializePool();
        InitializeMusicSource();
        _saveSystemManager = IServiceLocator.Instance.GetService<SaveSystemManager>();
        _saveSystemManager.RegisterSaveable(this);
        if (Application.isPlaying)
        {
            switch (SceneManager.GetActiveScene().name)
            {
                case string name when name == _mainMenuSceneName:
                    SetMasterVolume(0.8f);
                    SetMusicVolume(0.8f);
                    SetSFXVolume(0.8f);
                    PlayMusic("menu_music");
                    break;
                case string name when name == _gameSceneName:
                    PlayMusic("background_music");
                    break;
            }
        }
    }

    private void OnDestroy()
    {
        if (_saveSystemManager != null)
        {
            _saveSystemManager.UnregisterSaveable(this);
        }
    }

    private void InitializeDictionary()
    {
        _soundDictionary = new Dictionary<string, SoundData>();
        foreach (var sound in _sounds)
        {
            if (!_soundDictionary.ContainsKey(sound.id))
            {
                _soundDictionary.Add(sound.id, sound);
            }
        }
    }

    private void InitializePool()
    {
        _audioSourcePool = new Queue<AudioSource>();
        for (int i = 0; i < _poolSize; i++)
        {
            GameObject audioSourceObject = new GameObject($"AudioSource_{i}");
            audioSourceObject.transform.SetParent(transform);

            AudioSource source = audioSourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            _audioSourcePool.Enqueue(source);
        }
    }

    private void InitializeMusicSource()
    {
        GameObject musicObject = new GameObject($"MusicSource");
        musicObject.transform.SetParent(transform);

        _musicSource = musicObject.AddComponent<AudioSource>();
        _musicSource.loop = true;
        _musicSource.playOnAwake = false; 
    }

    public void PlaySound(string id)
    {
        if (!_soundDictionary.TryGetValue(id, out SoundData sound))
        {
            Debug.LogWarning($"Sound not found: {id}");
            return;
        } 
            
        AudioSource source = GetPooledSource();
        if (source == null) return;
        ConfigureSource(source, sound);
        source.spatialBlend = 0f;
        source.Play();

        StartCoroutine(ReturnToPoolWhenFinished(source));
    }

    public void PlaySoundAtPosition(string id, Vector3 position)
    {
        if (!_soundDictionary.TryGetValue(id, out SoundData sound)) 
            return;
        AudioSource source = GetPooledSource();
        if (source == null) return;
        ConfigureSource(source, sound);
        source.transform.position = position;
        source.spatialBlend = sound.is3D ? 1f : 0f;
        source.Play();

        StartCoroutine(ReturnToPoolWhenFinished(source));
    }

    public void PlayMusic(string id)
    {
        if (!_soundDictionary.TryGetValue(id, out SoundData sound)) 
            return;
        Debug.LogWarning(sound.name);
        Debug.LogWarning(sound.GetRandomClip());
        _musicSource.clip = sound.GetRandomClip();
        _musicSource.volume = sound.volume;
        _musicSource.outputAudioMixerGroup = sound.mixerGroup;
        _musicSource.Play();
    }

    private AudioSource GetPooledSource()
    {
        if (_audioSourcePool.Count > 0)
        {
            return _audioSourcePool.Dequeue();
        }
        else
        {
            Debug.LogWarning("No available audio sources in the pool!");
            return null;
        }
    }

    private void ConfigureSource(AudioSource source, SoundData sound)
    {
        source.clip = sound.GetRandomClip();
        source.volume = sound.volume;
        source.pitch = sound.pitch + Random.Range(-sound.pitchVariance, sound.pitchVariance);
        source.loop = sound.loop;
        source.outputAudioMixerGroup = sound.mixerGroup;

        // 3D sound settings
        source.spatialBlend = sound.is3D ? sound.spatialBlend : 0f;
        source.minDistance = sound.minDistance;
        source.maxDistance = sound.maxDistance;
        source.rolloffMode = sound.rolloffMode;
        source.dopplerLevel = sound.dopplerLevel;
        source.spread = sound.spread;
        source.priority = sound.priority;

        if (sound.useCustomRolloff && sound.customRolloff != null)
        {
            source.rolloffMode = AudioRolloffMode.Custom;
            source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, sound.customRolloff);
        }
        else
        {
            source.rolloffMode = sound.rolloffMode;
        }
    }

    private IEnumerator ReturnToPoolWhenFinished(AudioSource source)
    {
        yield return new WaitWhile(() => source.isPlaying);
        //source.clip = null;
        _audioSourcePool.Enqueue(source);
    }

    public void SetMasterVolume(float value) => SetVolume("MasterVolume", value);
    public void SetMusicVolume(float value) => SetVolume("MusicVolume", value);
    public void SetSFXVolume(float value) => SetVolume("SFXVolume", value);
    public float GetMasterVolume() => GetVolume("MasterVolume");
    public float GetMusicVolume() => GetVolume("MusicVolume");
    public float GetSFXVolume() => GetVolume("SFXVolume");

    private void SetVolume(string parameter, float volume)
    {
        float dB = volume > 0.0001f ? Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f : -80f;
        _audioMixer.SetFloat(parameter, dB);
    }

    private float GetVolume(string parameter)
    {
        if (_audioMixer.GetFloat(parameter, out float dB))
        {
            return Mathf.Pow(10f, dB / 20f);
        }
        return 0f;
    }

    public SaveData GetSaveData(SaveData data)
    {
        data.soundSettings = new SoundSettingsData();
        data.soundSettings.masterVolume = GetMasterVolume();
        data.soundSettings.musicVolume = GetMusicVolume();
        data.soundSettings.sfxVolume = GetSFXVolume();
        return data;
    }

    public void SetSaveData(SaveData data)
    {
        SetMasterVolume(data.soundSettings.masterVolume);
        SetMusicVolume(data.soundSettings.musicVolume);
        SetSFXVolume(data.soundSettings.sfxVolume);
    }

    public void PauseMusic()
    {
        if (_musicSource.isPlaying)
        {
            _musicSource.Pause();
        }
    }

    public void PlayMusic()
    {
        if (!_musicSource.isPlaying)
        {
            _musicSource.UnPause();
        }
    }

    public void PauseAll()
    {
        _musicSource.Pause();
        foreach (var source in _audioSourcePool)
        {
            if (source.isPlaying)
            {
                source.Pause();
            }
        }
    }

    public void ResumeAll()
    {
        _musicSource.UnPause();
        foreach (var source in _audioSourcePool)
        {
            if (source.clip != null && !source.isPlaying)
            {
                source.UnPause();
            }
        }
    }
}
