
using UnityEngine;

public class SoundListenter : MonoBehaviour
{
    private static GameObject _soundListener;
    private static AudioReverbZone _reverbZone;

    private static Transform TransForm => _soundListener ? _soundListener.transform : null;

    public static void Init()
    {
        if (!_soundListener)
        {
            _soundListener = ObjectHelper.CreateGameObject("SoundListener", typeof(AudioListener));
            _soundListener.AddComponent<SoundListenter>();
            _reverbZone = _soundListener.AddComponent<AudioReverbZone>();
            _soundListener.DontDestroyOnSceneChanged();
            EnabledReverbZone(false);
        }

        Sound.GlobalMusicVolume = XGameSetting.MusicVolume;
        Sound.GlobalAudioVolume = XGameSetting.AudioVolume;
    }

    public static void ChangeReverbZoneType(AudioReverbPreset type)
    {
        EnabledReverbZone(true);
        _reverbZone.reverbPreset = type;
    }

    public static void EnabledReverbZone(bool bEnable)
    {
        if (_reverbZone != null)
        {
            _reverbZone.enabled = bEnable;
        }
    }

    protected void LateUpdate()
    {
        var mainCamera = CameraHelper.Camera;
        if (mainCamera != null)
        {
            var trans = mainCamera.transform;
            TransForm.position = trans.position;
            TransForm.rotation = trans.rotation;
        }
    }
}