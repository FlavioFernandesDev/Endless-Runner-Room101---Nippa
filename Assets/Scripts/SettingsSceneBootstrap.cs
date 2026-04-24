using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class SettingsSceneBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        SettingsManager.LoadAndApply();

        if (Object.FindAnyObjectByType<SettingsSceneBootstrap>() != null)
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
        InstallAchievementsScene(scene);
        ApplyStageSelectLayout(scene);
        ApplyLocalization(scene);
    }

    private void InstallMainMenuSettings(Scene scene)
    {
        MainMenuControl menuControl = FindMainMenuControl(scene);
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
        TMP_Text[] texts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include);
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

        if (scene.name == RunManager.AchievementsSceneName)
        {
            AchievementsSceneController controller = FindInScene<AchievementsSceneController>(scene);
            if (controller != null)
            {
                controller.RefreshLocalizedContent();
            }
        }
    }

    private void InstallAchievementsScene(Scene scene)
    {
        if (scene.name != RunManager.AchievementsSceneName)
        {
            return;
        }

        AchievementsSceneController controller = FindInScene<AchievementsSceneController>(scene);
        if (controller == null)
        {
            GameObject controllerObject = new GameObject("AchievementsSceneController");
            SceneManager.MoveGameObjectToScene(controllerObject, scene);
            controller = controllerObject.AddComponent<AchievementsSceneController>();
        }

        controller.Initialize(scene);
    }

    private void ApplyStageSelectLayout(Scene scene)
    {
        if (scene.name != RunManager.StageSelectSceneName)
        {
            return;
        }

        StageControls stageControls = FindInScene<StageControls>(scene);
        RectTransform roomRunButton = ConfigureStageButton(scene, "SelectAndPlay", "stage.room_run", new Vector2(-380f, 102f));
        EnsureHauntedStageButton(scene, roomRunButton);
        RectTransform hauntedButton = ConfigureStageButton(scene, "HauntedHotel", "stage.haunted_hotel", new Vector2(0f, 102f));
        RectTransform quitButton = ConfigureStageButton(scene, "Sair", "stage.quit", new Vector2(380f, 102f));

        if (stageControls == null)
        {
            return;
        }

        ConfigureButtonClick(roomRunButton, stageControls.PlayRoomRun);
        ConfigureButtonClick(hauntedButton, stageControls.PlayHauntedHotel);
        ConfigureButtonClick(quitButton, stageControls.ReturnToMainMenu);
    }

    private RectTransform ConfigureStageButton(Scene scene, string buttonName, string localizationKey, Vector2 anchoredPosition)
    {
        RectTransform buttonRect = FindNamedComponent<RectTransform>(scene, buttonName);
        if (buttonRect == null)
        {
            return null;
        }

        buttonRect.anchorMin = new Vector2(0.5f, 0f);
        buttonRect.anchorMax = new Vector2(0.5f, 0f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = anchoredPosition;
        buttonRect.sizeDelta = new Vector2(330f, 118f);

        TMP_Text label = buttonRect.GetComponentInChildren<TMP_Text>(true);
        if (label == null)
        {
            return buttonRect;
        }

        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.pivot = new Vector2(0.5f, 0.5f);
        labelRect.anchoredPosition = Vector2.zero;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        label.margin = Vector4.zero;
        label.fontSize = 86f;
        label.enableAutoSizing = true;
        label.fontSizeMin = 34f;
        label.fontSizeMax = 86f;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.overflowMode = TextOverflowModes.Truncate;
        label.alignment = TextAlignmentOptions.Center;
        EnsureLocalized(label, localizationKey);
        return buttonRect;
    }

    private void EnsureHauntedStageButton(Scene scene, RectTransform templateButton)
    {
        if (FindNamedComponent<RectTransform>(scene, "HauntedHotel") != null || templateButton == null)
        {
            return;
        }

        GameObject hauntedButton = Object.Instantiate(templateButton.gameObject, templateButton.parent);
        hauntedButton.name = "HauntedHotel";
    }

    private void ConfigureButtonClick(RectTransform buttonRect, UnityAction action)
    {
        if (buttonRect == null || action == null)
        {
            return;
        }

        Button button = buttonRect.GetComponent<Button>();
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    private static T FindInScene<T>(Scene scene) where T : Component
    {
        T[] objects = Object.FindObjectsByType<T>(FindObjectsInactive.Include);
        foreach (T current in objects)
        {
            if (current != null && current.gameObject.scene == scene)
            {
                return current;
            }
        }

        return null;
    }

    private static T FindNamedComponent<T>(Scene scene, string objectName) where T : Component
    {
        T[] objects = Object.FindObjectsByType<T>(FindObjectsInactive.Include);
        foreach (T current in objects)
        {
            if (current != null && current.gameObject.scene == scene && current.gameObject.name == objectName)
            {
                return current;
            }
        }

        return null;
    }

    private static MainMenuControl FindMainMenuControl(Scene scene)
    {
        MainMenuControl[] controls = Object.FindObjectsByType<MainMenuControl>(FindObjectsInactive.Include);
        MainMenuControl fallback = null;

        foreach (MainMenuControl current in controls)
        {
            if (current == null || current.gameObject.scene != scene)
            {
                continue;
            }

            fallback ??= current;

            if (current.painelBotoesPrincipais != null)
            {
                return current;
            }
        }

        return fallback != null && fallback.painelBotoesPrincipais != null ? fallback : null;
    }

    private static void EnsureLocalized(TMP_Text text, string key)
    {
        LocalizedText localizedText = text.GetComponent<LocalizedText>();
        if (localizedText == null)
        {
            localizedText = text.gameObject.AddComponent<LocalizedText>();
        }

        localizedText.AssignKey(key);
    }
}
