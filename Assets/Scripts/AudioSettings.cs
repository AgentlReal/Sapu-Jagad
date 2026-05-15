using UnityEngine;

public static class AudioSettings
{
    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    public static float MasterVolume
    {
        get => PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
        set
        {
            PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, value);
            AudioListener.volume = value;
            PlayerPrefs.Save();
        }
    }

    public static float SFXVolume
    {
        get => PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
        set
        {
            PlayerPrefs.SetFloat(SFX_VOLUME_KEY, value);
            PlayerPrefs.Save();
        }
    }

    public static void Initialize()
    {
        AudioListener.volume = MasterVolume;
    }
}
