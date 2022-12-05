using UnityEngine;

public static class LocalStorage
{
    public enum Key
    {
        DeviceId = 101,
        System = 102,
    }

    public static int ReadInt(Key key)
    {
        return PlayerPrefs.GetInt(key.ToString());
    }

    public static void WriteInt(Key key, int value)
    {
        PlayerPrefs.SetInt(key.ToString(), value);
        PlayerPrefs.Save();
    }

    public static float ReadFloat(Key key)
    {
        return PlayerPrefs.GetFloat(key.ToString());
    }

    public static void WriteFloat(Key key, float value)
    {
        PlayerPrefs.SetFloat(key.ToString(), value);
        PlayerPrefs.Save();
    }

    public static string Read(Key key)
    {
        return PlayerPrefs.GetString(key.ToString());
    }

    public static void Write(Key key, string value)
    {
        PlayerPrefs.SetString(key.ToString(), value);
        PlayerPrefs.Save();
    }

    public static void Remove(Key key)
    {
        PlayerPrefs.DeleteKey(key.ToString());
        PlayerPrefs.Save();
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}