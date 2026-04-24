using System;
using UnityEngine;

public static class SettingsManager
{
    private const string MasterVolumeKey = "SETTINGS_MASTER_VOLUME";
    private const string LanguageKey = "SETTINGS_LANGUAGE";
    private const string FullscreenKey = "SETTINGS_FULLSCREEN";
    private const string QualityKey = "SETTINGS_QUALITY";

    private static bool _loaded;

    public static event Action LanguageChanged;

    public static float MasterVolume { get; private set; } = 1f;
    public static AppLanguage Language { get; private set; } = AppLanguage.EN;
    public static bool Fullscreen { get; private set; } = true;
    public static int QualityLevel { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        LoadAndApply();
    }

    public static void LoadAndApply()
    {
        if (_loaded)
        {
            ApplyAll();
            return;
        }

        _loaded = true;

        MasterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumeKey, 1f));
        Language = LoadLanguage();
        Fullscreen = PlayerPrefs.GetInt(FullscreenKey, 1) == 1;
        QualityLevel = LoadQualityLevel();

        ApplyAll();
    }

    public static void SetMasterVolume(float value)
    {
        LoadAndApply();
        MasterVolume = Mathf.Clamp01(value);
        AudioListener.volume = MasterVolume;
        PlayerPrefs.SetFloat(MasterVolumeKey, MasterVolume);
        PlayerPrefs.Save();
    }

    public static void SetLanguage(AppLanguage language)
    {
        LoadAndApply();
        if (Language == language)
        {
            return;
        }

        Language = language;
        PlayerPrefs.SetInt(LanguageKey, (int)Language);
        PlayerPrefs.Save();
        LanguageChanged?.Invoke();
    }

    public static void SetFullscreen(bool value)
    {
        LoadAndApply();
        Fullscreen = value;
        Screen.fullScreen = Fullscreen;
        PlayerPrefs.SetInt(FullscreenKey, Fullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static void SetQuality(int qualityLevel)
    {
        LoadAndApply();
        int[] qualityLevels = GetAvailableQualityLevels();
        if (qualityLevels.Length == 0)
        {
            return;
        }

        int clampedIndex = Mathf.Clamp(qualityLevel, 0, qualityLevels.Length - 1);
        QualityLevel = qualityLevels[clampedIndex];
        QualitySettings.SetQualityLevel(QualityLevel, true);
        PlayerPrefs.SetInt(QualityKey, QualityLevel);
        PlayerPrefs.Save();
    }

    public static int[] GetAvailableQualityLevels()
    {
        int qualityCount = QualitySettings.names.Length;
        int[] qualityLevels = new int[qualityCount];
        for (int i = 0; i < qualityCount; i++)
        {
            qualityLevels[i] = i;
        }

        return qualityLevels;
    }

    private static AppLanguage LoadLanguage()
    {
        int savedLanguage = PlayerPrefs.GetInt(LanguageKey, (int)AppLanguage.EN);
        if (!Enum.IsDefined(typeof(AppLanguage), savedLanguage))
        {
            return AppLanguage.EN;
        }

        return (AppLanguage)savedLanguage;
    }

    private static int LoadQualityLevel()
    {
        int currentQuality = QualitySettings.GetQualityLevel();
        int qualityLevel = PlayerPrefs.GetInt(QualityKey, currentQuality);
        int[] availableLevels = GetAvailableQualityLevels();
        if (availableLevels.Length == 0)
        {
            return 0;
        }

        return Mathf.Clamp(qualityLevel, 0, availableLevels.Length - 1);
    }

    private static void ApplyAll()
    {
        AudioListener.volume = MasterVolume;
        Screen.fullScreen = Fullscreen;
        QualitySettings.SetQualityLevel(QualityLevel, true);
        ApplyFramePacing();
    }

    private static void ApplyFramePacing()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;
#else
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
#endif
    }
}
