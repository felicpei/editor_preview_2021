using LitJson;
using UnityEngine;

public static class SystemDataMgr
{
    private static SystemData _data;

    public static SystemData Data
    {
        get
        {
            if (_data == null)
            {
                LoadSystemData();
            }

            return _data;
        }
    }

    public static void SaveSystemData()
    {
        LocalStorage.Write(LocalStorage.Key.System, JsonMapper.ToJson(Data));
        Debug.Log("Save SystemData!");
    }

    public static void LoadSystemData()
    {
        var source = LocalStorage.Read(LocalStorage.Key.System);
        if (string.IsNullOrEmpty(source))
        {
            CreateSystemData();
            return;
        }

        _data = JsonMapper.ToObject<SystemData>(source);
    }

    public static void CreateSystemData()
    {
        _data = new SystemData
        {
            LatestFileNo = 1,
            ContinueFileNo = 1,
            SettingData = new SettingData
            {
                MusicVolume = 1f,
                AudioVolume = 1f,
                Shake = true,
                Resolution = XGameSetting.EnumResolution.Mid,
            }
        };
        SaveSystemData();
    }

    public static void Save(this SettingData settingData)
    {
        SaveSystemData();
    }

    public static void Save(this SystemData data)
    {
        SaveSystemData();
    }
}