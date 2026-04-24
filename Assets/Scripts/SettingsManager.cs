using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine;

public static class SettingsManager
{
    public const int LowQualityPreset = 0;
    public const int MediumQualityPreset = 1;
    public const int HighQualityPreset = 2;
    public const int QualityPresetCount = 3;

    private const string MasterVolumeKey = "SETTINGS_MASTER_VOLUME";
    private const string LanguageKey = "SETTINGS_LANGUAGE";
    private const string FullscreenKey = "SETTINGS_FULLSCREEN";
    private const string QualityKey = "SETTINGS_QUALITY";
    private const int DefaultQualityPreset = MediumQualityPreset;

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
        QualityLevel = ClampQualityPreset(qualityLevel);
        ApplyQualityPreset();
        PlayerPrefs.SetInt(QualityKey, QualityLevel);
        PlayerPrefs.Save();
    }

    public static int[] GetAvailableQualityPresets()
    {
        int[] qualityPresets = new int[QualityPresetCount];
        for (int i = 0; i < qualityPresets.Length; i++)
        {
            qualityPresets[i] = i;
        }

        return qualityPresets;
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
        int qualityLevel = PlayerPrefs.GetInt(QualityKey, DefaultQualityPreset);
        return ClampQualityPreset(qualityLevel);
    }

    private static void ApplyAll()
    {
        AudioListener.volume = MasterVolume;
        Screen.fullScreen = Fullscreen;
        ApplyQualityPreset();
        ApplyFramePacing();
    }

    private static int ClampQualityPreset(int qualityLevel)
    {
        return Mathf.Clamp(qualityLevel, LowQualityPreset, HighQualityPreset);
    }

    private static void ApplyQualityPreset()
    {
        QualityPreset preset = GetQualityPreset(QualityLevel);
        ApplyClosestUnityQualityLevel();

        QualitySettings.lodBias = preset.LodBias;
        QualitySettings.shadowDistance = preset.ShadowDistance;
        QualitySettings.antiAliasing = preset.MsaaSamples;
        QualitySettings.globalTextureMipmapLimit = preset.TextureMipmapLimit;
        QualitySettings.shadows = preset.Shadows;
        QualitySettings.shadowResolution = preset.ShadowResolution;

        ApplyUrpQuality(preset);
    }

    private static void ApplyClosestUnityQualityLevel()
    {
        int qualityCount = QualitySettings.names.Length;
        if (qualityCount == 0)
        {
            return;
        }

        int unityQualityLevel = Mathf.Clamp(QualityLevel, 0, qualityCount - 1);
        QualitySettings.SetQualityLevel(unityQualityLevel, true);
    }

    private static void ApplyUrpQuality(QualityPreset preset)
    {
        RenderPipelineAsset renderPipelineAsset = QualitySettings.renderPipeline;
        if (renderPipelineAsset == null)
        {
            renderPipelineAsset = GraphicsSettings.currentRenderPipeline;
        }

        if (renderPipelineAsset is not UniversalRenderPipelineAsset urpAsset)
        {
            return;
        }

        urpAsset.renderScale = preset.RenderScale;
        urpAsset.msaaSampleCount = preset.MsaaSamples;
        urpAsset.shadowDistance = preset.ShadowDistance;
    }

    private static QualityPreset GetQualityPreset(int qualityLevel)
    {
        return ClampQualityPreset(qualityLevel) switch
        {
            LowQualityPreset => new QualityPreset(0.75f, 0.75f, 18f, 1, 1, UnityEngine.ShadowQuality.Disable, UnityEngine.ShadowResolution.Low),
            HighQualityPreset => new QualityPreset(1f, 2f, 35f, 4, 0, UnityEngine.ShadowQuality.All, UnityEngine.ShadowResolution.High),
            _ => new QualityPreset(0.9f, 1f, 25f, 1, 0, UnityEngine.ShadowQuality.HardOnly, UnityEngine.ShadowResolution.Medium)
        };
    }

    private readonly struct QualityPreset
    {
        public QualityPreset(float renderScale, float lodBias, float shadowDistance, int msaaSamples, int textureMipmapLimit, UnityEngine.ShadowQuality shadows, UnityEngine.ShadowResolution shadowResolution)
        {
            RenderScale = renderScale;
            LodBias = lodBias;
            ShadowDistance = shadowDistance;
            MsaaSamples = msaaSamples;
            TextureMipmapLimit = textureMipmapLimit;
            Shadows = shadows;
            ShadowResolution = shadowResolution;
        }

        public float RenderScale { get; }
        public float LodBias { get; }
        public float ShadowDistance { get; }
        public int MsaaSamples { get; }
        public int TextureMipmapLimit { get; }
        public UnityEngine.ShadowQuality Shadows { get; }
        public UnityEngine.ShadowResolution ShadowResolution { get; }
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
