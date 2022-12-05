//////////////////////////////////////////////////////////////////////////
//
//   FileName : Sound.cs
//     Author : Chiyer
// CreateTime : 2014-04-28
//       Desc :
//
//////////////////////////////////////////////////////////////////////////

using System;
using UnityEngine;
using System.Collections.Generic;
using Object = UnityEngine.Object;

//对应SOUND表ID
public enum UISoundId
{
}

public static class Sound
{
    private class MusicAudioSource
    {
        public AudioSource AudioSource;
        public string Resources;
        public bool InPause;
    }

    private static MusicAudioSource _music;
    private static MusicAudioSource _music2;
    private static MusicAudioSource _environment;

    private static float _musicVolume;
    private static float _environmentVolume;

    private static float _globalMusicVolume;
    private static float _globalAudioVolume;

    private static AudioSource _uiAudio;
    private static TableT<SoundDeploy> _soundTableT;

    private static bool _bMusicPause;
    private static bool _bMusic2Pause;
    private static bool _bEnvironmentPause;

    private static bool _bInit;
    public static void Init()
    {
        if (_bInit)
        {
            return;
        }

        _bInit = true;
        SoundListenter.Init();
    }
    
    private static void AdPause(MusicAudioSource source)
    {
        if (source != null && source.AudioSource != null)
        {
            source.AudioSource.Pause();
        }
    }

    private static void AdResume(MusicAudioSource source)
    {
        if (source != null && source.AudioSource != null)
        {
            if (!source.InPause)
            {
                source.AudioSource.UnPause();
            }
        }
    }

    public static void CloseSound()
    {
        GlobalAudioVolume = 0;

        AdPause(_music);
        AdPause(_music2);
        AdPause(_environment);

        Debug.Log("CloseSound ad");
    }

    public static void OpenSound()
    {
        GlobalAudioVolume = XGameSetting.AudioVolume;

        AdResume(_music);
        AdResume(_music2);
        AdResume(_environment);
        Debug.Log("OpenSound ad， GlobalAudioVolume：" + GlobalAudioVolume + " globalMusic:" + GlobalMusicVolume);
    }


    public static float GlobalMusicVolume
    {
        get => _globalMusicVolume;
        set
        {
            _globalMusicVolume = value;
            RefreshMusicVolume();
        }
    }

    private static void RefreshMusicVolume()
    {
        if (_music != null && _music.AudioSource != null)
        {
            _music.AudioSource.volume = _musicVolume * _globalMusicVolume;
        }

        if (_environment != null && _environment.AudioSource != null)
        {
            _environment.AudioSource.volume = _environmentVolume * _globalMusicVolume;
        }
    }

    public static float GlobalAudioVolume
    {
        get => _globalAudioVolume;
        set => _globalAudioVolume = value;
    }

    static Sound()
    {
        _musicVolume = 1f;
        _environmentVolume = 1f;
        GlobalMusicVolume = 1f;
        GlobalAudioVolume = 1f;
        _soundTableT = TableMgr.GetTable<SoundDeploy>();
    }

    public static AudioSource Create3DAudioSource(GameObject parent, float maxDistance = 50, bool useLinear = false)
    {
        var audioSource = parent.AddComponent<AudioSource>();
        audioSource.rolloffMode = AudioRolloffMode.Linear; //useLinear ? AudioRolloffMode.Linear : AudioRolloffMode.Logarithmic;
        audioSource.minDistance = 10;
        audioSource.maxDistance = maxDistance;
        audioSource.reverbZoneMix = 1f; //混响比例
        audioSource.spatialize = true; //空间声音
        audioSource.spatialBlend = 1f; //3d音效比例
        audioSource.spread = 300; //传播角度,影响左右声道
        audioSource.reverbZoneMix = 1f;
        return audioSource;
    }

    public static AudioSource Create2DAudioSource(GameObject parent)
    {
        var source = parent.AddComponent<AudioSource>();
        source.reverbZoneMix = 0; //混响比例
        source.spatialize = false; //空间声音
        source.spatialBlend = 0; //3d音效比例
        source.spread = 0; //传播角度,影响左右声道
        return source;
    }

    public static void Load(int id, Action<SoundClip> notify)
    {
        var deploy = _soundTableT.GetSection(id);
        if (deploy == null || string.IsNullOrEmpty(deploy.resource))
        {
            return;
        }

        Load(deploy.resource, deploy.volume, notify);
    }

    private static void Load(string resource, float volume = 1f, Action<SoundClip> notify = null)
    {
        LoadSoundResource(resource, audioClip =>
        {
            SoundClip soundClip = null;
            if (audioClip == null)
            {
                Dbg.LogError(string.Format("The AudioClip[resource={0}] is null.", resource));
            }
            else
            {
                soundClip = new SoundClip(audioClip, volume);
            }

            notify?.Invoke(soundClip);
        });
    }

    private static void Create2DAudioSource(string resource, float volume = 1f, Action<AudioSource> notify = null)
    {
        LoadSoundResource(resource, clipObject =>
        {
            if (clipObject)
            {
                var sourceObject = new GameObject("2DAudioSource");
                var source = sourceObject.AddComponent<AudioSource>();
                source.reverbZoneMix = 0; //混响比例
                source.spatialize = false; //空间声音
                source.spatialBlend = 0; //3d音效比例
                source.spread = 0; //传播角度,影响左右声道
                source.clip = clipObject;
                source.volume = volume;
                notify?.Invoke(source);
            }
            else if (notify != null)
            {
                Dbg.LogError(string.Format("The AudioSource[resource={0}] is null.", resource));
                notify(null);
            }
        });
    }


    public static void StopEnvironmentMusic(float fade = 3f)
    {
        if (_environment != null && _environment.AudioSource != null)
        {
            CachePool.Remove(_environment.Resources);
            Resources.UnloadAsset(_environment.AudioSource.clip);
            Object.Destroy(_environment.AudioSource.gameObject);
        }
    }

    public static void PlayEnvironmentMusic(int id, bool loop = true, float fade = 1f)
    {
        var deploy = _soundTableT.GetSection(id);
        if (deploy == null)
        {
            return;
        }

        PlayEnvironmentMusic(deploy.resource, loop, deploy.volume, fade);
    }

    public static void PlayEnvironmentMusic(string resource, bool loop = true, float volume = 1f, float fade = 1f)
    {
        Create2DAudioSource(resource, volume, sourceObject =>
        {
            if (sourceObject)
            {
                sourceObject.volume = volume * _globalMusicVolume;

                if (_environment == null)
                {
                    _environment = new MusicAudioSource();
                }

                if (_environment.AudioSource != null)
                {
                    Object.Destroy(_environment.AudioSource.gameObject);
                }


                _environment.Resources = resource;
                _environment.AudioSource = sourceObject;
                _environmentVolume = volume;
                _environment.AudioSource.loop = loop;
                _environment.AudioSource.name = "EnvironmentAudio";
                _environment.AudioSource.Play();
                _environment.AudioSource.gameObject.DontDestroyOnSceneChanged();
            }
            else if (_environment != null && _environment.AudioSource != null)
            {
                Object.Destroy(_environment.AudioSource.gameObject);
                _environment.AudioSource = null;
            }
        });
    }

    public static void PauseMusic(bool b)
    {
        if (_music != null && _music.AudioSource)
        {
            if (b)
            {
                _music.AudioSource.Pause();
                _music.InPause = true;
            }
            else
            {
                _music.AudioSource.UnPause();
                _music.InPause = false;
            }
        }
    }

    public static int CurrentMusicId { private set; get; }

    public static void PlayMusic(int id, bool loop = true, string tag = "")
    {
        var deploy = _soundTableT.GetSection(id);
        if (deploy == null)
        {
            return;
        }

        CurrentMusicId = id;
        PlayMusic(deploy.resource, tag, loop, deploy.volume);
    }

    public static void PlayMusic(string resource, string name = null, bool loop = true, float volume = 1f)
    {
        Create2DAudioSource(resource, volume, sourceObject =>
        {
            if (sourceObject)
            {
                sourceObject.volume = volume * _globalMusicVolume;

                if (_music == null)
                {
                    _music = new MusicAudioSource();
                }

                if (_music.AudioSource != null)
                {
                    Object.Destroy(_music.AudioSource.gameObject);
                }

                _music.Resources = resource;
                _music.AudioSource = sourceObject;
                _musicVolume = volume;
                _music.AudioSource.loop = loop;
                _music.AudioSource.name = "BgmAudio";
                _music.AudioSource.Play();
                _music.AudioSource.gameObject.DontDestroyOnSceneChanged();
            }
            else if (_music != null && _music.AudioSource != null)
            {
                Object.Destroy(_music.AudioSource.gameObject);
                _music.AudioSource = null;
            }
        });
    }

    public static void StopMusic()
    {
        if (_music != null && _music.AudioSource != null)
        {
            CachePool.Remove(_music.Resources);
            Resources.UnloadAsset(_music.AudioSource.clip);
            Object.Destroy(_music.AudioSource.gameObject);
            _music.AudioSource = null;
        }
    }


    public static void PlayMusic2(int id, bool loop = true)
    {
        var deploy = _soundTableT.GetSection(id);
        if (deploy == null)
        {
            return;
        }

        PlayMusic2(deploy.resource, loop, deploy.volume);
    }

    public static void PlayMusic2(string resource, bool loop = true, float volume = 1f)
    {
        Create2DAudioSource(resource, volume, sourceObject =>
        {
            if (sourceObject)
            {
                sourceObject.volume = volume * _globalMusicVolume;

                if (_music2 == null)
                {
                    _music2 = new MusicAudioSource();
                }

                if (_music2.AudioSource != null)
                {
                    Object.Destroy(_music2.AudioSource.gameObject);
                }

                _music2.Resources = resource;
                _music2.AudioSource = sourceObject;
                _music2.AudioSource.loop = loop;
                _music2.AudioSource.name = "BgmAudio";
                _music2.AudioSource.Play();
                _music2.AudioSource.gameObject.DontDestroyOnSceneChanged();
            }
            else if (_music2 != null && _music2.AudioSource != null)
            {
                Object.Destroy(_music2.AudioSource.gameObject);
                _music2.AudioSource = null;
            }
        });
    }

    public static void StopMusic2()
    {
        if (_music2 != null && _music2.AudioSource != null)
        {
            CachePool.Remove(_music2.Resources);
            Resources.UnloadAsset(_music2.AudioSource.clip);
            Object.Destroy(_music2.AudioSource.gameObject);
            _music2.AudioSource = null;
            _music2 = null;
        }
    }


    public static void PauseMusic2(bool b)
    {
        if (_music2 != null && _music2.AudioSource)
        {
            if (b)
            {
                _music2.AudioSource.Pause();
                _music2.InPause = true;
            }
            else
            {
                _music2.AudioSource.UnPause();
                _music2.InPause = false;
            }
        }
    }

    public static void UnloadSoundAsset(AudioSource audioSource, int soundId)
    {
        if (audioSource != null)
        {
            var deploy = _soundTableT.GetSection(soundId);
            CachePool.Remove(deploy.resource);
            Resources.UnloadAsset(audioSource.clip);
        }
    }

    #region PlayAudioOneShot

    private static readonly Dictionary<int, float> OneShotCd = new();

    public static void PlayAudioOneShot(AudioSource audioSource, int soundId, float volume = 1f)
    {
        if (_globalAudioVolume <= 0)
        {
            return;
        }

        var deploy = _soundTableT.GetSection(soundId);
        if (deploy == null || string.IsNullOrEmpty(deploy.resource))
        {
            return;
        }

        if (OneShotCd.TryGetValue(soundId, out var lastTime))
        {
            if (Time.time - lastTime < 0.05f)
            {
                return;
            }
        }

        OneShotCd[soundId] = Time.time;

        if (CachePool.TryGetValue(deploy.resource, out var resource))
        {
            PlayAudioOneShot(audioSource, resource, volume, deploy.volume);
        }
        else
        {
            RLoadPlayAudioOneShot(deploy.resource, audioSource, volume, deploy.volume);
        }
    }

    private static void RLoadPlayAudioOneShot(string soundName, AudioSource audioSource, float volume = 1f, float gvolume = 1f)
    {
        AudioClip resource;
        XResource.Load(soundName, obj =>
        {
            resource = obj as AudioClip;
            if (resource != null)
            {
                if (!CachePool.ContainsKey(soundName))
                {
                    CachePool.Add(soundName, resource);
                }

                if (audioSource != null)
                {
                    PlayAudioOneShot(audioSource, resource, volume, gvolume);
                }
            }
            else
            {
                Dbg.LogError(string.Format("The AudioClip[resource={0}] is null.", resource));
            }
        });
    }

    private static void PlayAudioOneShot(AudioSource audioSource, AudioClip clip, float volume = 1f, float gvolume = 1f)
    {
        if (audioSource == null) return;
        if (!audioSource.gameObject.activeInHierarchy) return;
        audioSource.PlayOneShot(clip, volume * _globalAudioVolume * gvolume);
    }

    #endregion

    #region PlayAudio

    public static void PlayAudio(AudioSource audioSource, int soundId, bool bLoop = true, Action playCallBack = null)
    {
        if (audioSource == null) return;
        LoadPlayAudio(soundId, audioSource, bLoop, playCallBack);
    }

    private static void LoadPlayAudio(int id, AudioSource audioSource, bool bLoop, Action playCallBack)
    {
        if (_globalAudioVolume <= 0)
            return;

        var deploy = _soundTableT.GetSection(id);
        if (deploy == null || string.IsNullOrEmpty(deploy.resource))
        {
            return;
        }

        LoadPlayAudio(deploy.resource, audioSource, deploy.volume, bLoop, playCallBack);
    }

    private static void LoadPlayAudio(string soundName, AudioSource audioSource, float volume, bool loop, Action playCallBack)
    {
        if (CachePool.TryGetValue(soundName, out var resource))
        {
            DoPlayAudio(audioSource, resource, volume, loop, playCallBack);
        }
        else
        {
            RLoadPlayAudio(soundName, audioSource, volume, loop, playCallBack);
        }
    }

    private static void RLoadPlayAudio(string soundName, AudioSource audioSource, float volume, bool loop, Action playCallBack)
    {
        AudioClip resource;
        XResource.Load(soundName, obj =>
        {
            resource = obj as AudioClip;
            if (resource)
            {
                if (!CachePool.ContainsKey(soundName))
                {
                    CachePool.Add(soundName, resource);
                }

                DoPlayAudio(audioSource, resource, volume, loop, playCallBack);
            }
            else
            {
                Debug.LogError("加载声音失败:" + soundName);
            }
        });
    }

    private static void DoPlayAudio(AudioSource audioSource, AudioClip clip, float volume, bool loop, Action playCallBack)
    {
        if (audioSource == null || clip == null) return;
        if (!audioSource.gameObject.activeInHierarchy) return;

        audioSource.playOnAwake = false;
        audioSource.loop = loop;
        if (!loop)
        {
            audioSource.PlayOneShot(clip, volume * _globalAudioVolume);
        }
        else
        {
            audioSource.clip = clip;
            audioSource.volume = volume * _globalAudioVolume;
            audioSource.Play();
        }


        playCallBack?.Invoke();
    }

    public static void StopAudio(AudioSource audioSource)
    {
        if (audioSource)
        {
            audioSource.Stop();
        }
    }

    #endregion

    public static void PlayUIAudioOneShot(UISoundId id)
    {
        PlayUiAudioOneShot((int)id);
    }

    public static void PlayUiAudioOneShot(int soundId)
    {
        Load(soundId, PlayUiAudioOneShot);
    }

    private static void PlayUiAudioOneShot(SoundClip soundClip)
    {
        //Dbg.Log("PlayUiAudioOneShot:" + soundClip.clip.name + " volume:" + soundClip.volume + " uiv:" + _uiVolume + "_globalv:" + _globalAudioVolume);
        if (_globalAudioVolume <= 0)
        {
            return;
        }

        if (_uiAudio == null)
        {
            CreateUiAudioSource();
        }

        if (_uiAudio != null)
        {
            _uiAudio.PlayOneShot(soundClip.Clip, soundClip.Volume * _globalAudioVolume);
        }
    }

    private static void CreateUiAudioSource()
    {
        var gameObject = new GameObject("UI Audio Source");
        Object.DontDestroyOnLoad(gameObject);
        _uiAudio = gameObject.AddComponent<AudioSource>();
        _uiAudio.playOnAwake = false;
        _uiAudio.reverbZoneMix = 0; //混响比例
        _uiAudio.spatialize = false; //空间声音
        _uiAudio.spatialBlend = 0; //3d音效比例
        _uiAudio.spread = 0; //传播角度,影响左右声道
    }

    public static void ClearSoundCache()
    {
        var enumerator = CachePool.GetEnumerator();
        using (enumerator)
        {
            while (enumerator.MoveNext())
            {
                Resources.UnloadAsset(enumerator.Current.Value);
            }
        }

        CachePool.Clear();
    }

    private static readonly Dictionary<string, AudioClip> CachePool = new();

    private static void LoadSoundResource(string soundName, Action<AudioClip> notify)
    {
        LoadSoundResourceImpl(soundName, notify);
    }

    private static void LoadSoundResourceImpl(string soundName, Action<AudioClip> notify)
    {
        if (CachePool.TryGetValue(soundName, out var resource))
        {
            notify(resource);
        }
        else
        {
            XResource.Load(soundName, _object =>
            {
                resource = _object as AudioClip;
                if (!resource)
                {
                    Debug.LogError("加载声音失败:" + soundName);
                }
                else
                {
                    if (!CachePool.ContainsKey(soundName))
                    {
                        CachePool.Add(soundName, resource);
                    }

                    notify(resource);
                }
            });
        }
    }

    public static void ClearAll()
    {
        StopEnvironmentMusic();
        StopMusic();
        StopMusic2();
        ClearSoundCache();
    }
}