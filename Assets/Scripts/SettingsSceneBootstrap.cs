using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SettingsSceneBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        SettingsManager.LoadAndApply();

        if (Object.FindFirstObjectByType<SettingsSceneBootstrap>() != null)
        {
            return;
        }

        GameObject bootstrapObject = new GameObject("SettingsSceneBootstrap");
        Object.DontDestroyOnLoad(bootstrapObject);
        bootstrapObject.AddComponent<SettingsSceneBootstrap>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
        SettingsManager.LanguageChanged += RefreshActiveSceneLocalization;
        ApplyScene(SceneManager.GetActiveScene());
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SettingsManager.LanguageChanged -= RefreshActiveSceneLocalization;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        ApplyScene(scene);
    }

    private void RefreshActiveSceneLocalization()
    {
        ApplyLocalization(SceneManager.GetActiveScene());
    }

    private void ApplyScene(Scene scene)
    {
        SettingsManager.LoadAndApply();
        InstallMainMenuSettings(scene);
        ApplyLocalization(scene);
    }

    private void InstallMainMenuSettings(Scene scene)
    {
        MainMenuControl menuControl = FindInScene<MainMenuControl>(scene);
        if (menuControl == null)
        {
            return;
        }

        SettingsMenuController settingsController = menuControl.GetComponent<SettingsMenuController>();
        if (settingsController == null)
        {
            settingsController = menuControl.gameObject.AddComponent<SettingsMenuController>();
        }

        settingsController.Initialize(menuControl);
    }

    private void ApplyLocalization(Scene scene)
    {
        TMP_Text[] texts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (TMP_Text text in texts)
        {
            if (text == null || text.gameObject.scene != scene)
            {
                continue;
            }

            LocalizedText localizedText = text.GetComponent<LocalizedText>();
            if (localizedText != null)
            {
                localizedText.Refresh();
                continue;
            }

            if (!LocalizationTable.TryGetKeyForValue(text.text, out string key))
            {
                continue;
            }

            localizedText = text.gameObject.AddComponent<LocalizedText>();
            localizedText.AssignKey(key);
        }
    }

    private static T FindInScene<T>(Scene scene) where T : Component
    {
        T[] objects = Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (T current in objects)
        {
            if (current != null && current.gameObject.scene == scene)
            {
                return current;
            }
        }

        return null;
    }
}
