using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class SettingsMenuSetupTool
{
    private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string StageSelectScenePath = "Assets/Scenes/StageSelect.unity";
    private const string InformationScenePath = "Assets/Scenes/Information.unity";

    [MenuItem("Tools/UI/Setup Settings Menu")]
    public static void RunBatchSetup()
    {
        SetupMainMenu();
        SetupStageSelect();
        SetupInformation();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Settings menu setup completed.");
    }

    private static void SetupMainMenu()
    {
        Scene scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
        GameObject mainMenuControlsObject = GameObject.Find("MainMenuControls");
        GameObject canvasObject = GameObject.Find("Canvas");
        if (mainMenuControlsObject == null || canvasObject == null)
        {
            Debug.LogWarning("MainMenu setup skipped because required root objects were not found.");
            return;
        }

        MainMenuControl menuControl = mainMenuControlsObject.GetComponent<MainMenuControl>();
        SettingsMenuController settingsController = mainMenuControlsObject.GetComponent<SettingsMenuController>();
        if (settingsController == null)
        {
            settingsController = mainMenuControlsObject.AddComponent<SettingsMenuController>();
        }

        Transform menuButtonsRoot = menuControl != null && menuControl.painelBotoesPrincipais != null
            ? menuControl.painelBotoesPrincipais.transform
            : FindChildRecursive(canvasObject.transform, "StartGame")?.parent;
        if (menuButtonsRoot == null)
        {
            Debug.LogWarning("MainMenu setup skipped because button panel root was not found.");
            return;
        }

        GameObject startButton = FindChildRecursive(menuButtonsRoot, "StartGame")?.gameObject;
        GameObject quitButton = FindChildRecursive(menuButtonsRoot, "Sair")?.gameObject;
        TMP_Text promptText = menuControl != null && menuControl.mensagemCliqueInicial != null
            ? menuControl.mensagemCliqueInicial.GetComponent<TMP_Text>()
            : null;

        if (startButton == null || quitButton == null)
        {
            Debug.LogWarning("MainMenu setup skipped because StartGame or Sair buttons were not found.");
            return;
        }

        SetupLocalizedText(startButton.GetComponentInChildren<TMP_Text>(true), "menu.start");
        SetupLocalizedText(quitButton.GetComponentInChildren<TMP_Text>(true), "menu.quit");
        SetupLocalizedText(promptText, "menu.game_over_prompt");

        GameObject settingsButton = FindChildRecursive(menuButtonsRoot, "SettingsButton")?.gameObject;
        if (settingsButton == null)
        {
            settingsButton = Object.Instantiate(startButton, menuButtonsRoot);
            settingsButton.name = "SettingsButton";
        }

        ConfigureMainMenuButton(settingsButton, new Vector2(0f, -4f), new Color(0.58f, 0.84f, 1f, 1f));
        SetupLocalizedText(settingsButton.GetComponentInChildren<TMP_Text>(true), "menu.settings");

        Button settingsButtonComponent = settingsButton.GetComponent<Button>();
        settingsButtonComponent.onClick = new Button.ButtonClickedEvent();
        UnityEventTools.AddPersistentListener(settingsButtonComponent.onClick, settingsController.OpenPanel);

        GameObject settingsPanel = FindChildRecursive(canvasObject.transform, "SettingsPanel")?.gameObject;
        if (settingsPanel == null)
        {
            settingsPanel = CreateSettingsPanel(canvasObject.transform, settingsController, startButton);
        }

        settingsPanel.SetActive(false);
        AssignSettingsControllerReferences(settingsController, settingsPanel);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void SetupStageSelect()
    {
        Scene scene = EditorSceneManager.OpenScene(StageSelectScenePath, OpenSceneMode.Single);
        SetupLocalizedTextByCurrentText("PLAY", "stage.play");
        SetupLocalizedTextByCurrentText("Room Run", "stage.room_run");
        SetupLocalizedTextByCurrentText("QUIT", "stage.quit");
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void SetupInformation()
    {
        Scene scene = EditorSceneManager.OpenScene(InformationScenePath, OpenSceneMode.Single);
        SetupLocalizedTextByCurrentText("JUMP", "info.jump");
        SetupLocalizedTextByCurrentText("MOVE LEFT", "info.move_left");
        SetupLocalizedTextByCurrentText("MOVE RIGHT", "info.move_right");
        SetupLocalizedTextByCurrentText("SPACEBAR", "info.spacebar");
        SetupLocalizedTextByCurrentText("LOADING...", "info.loading");
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static GameObject CreateSettingsPanel(Transform canvasTransform, SettingsMenuController controller, GameObject buttonTemplate)
    {
        GameObject panelRoot = CreateUiObject("SettingsPanel", canvasTransform, typeof(Image));
        RectTransform panelRect = panelRoot.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image overlay = panelRoot.GetComponent<Image>();
        overlay.color = new Color(0f, 0f, 0f, 0.65f);

        GameObject window = CreateUiObject("Window", panelRoot.transform, typeof(Image));
        RectTransform windowRect = window.GetComponent<RectTransform>();
        windowRect.anchorMin = new Vector2(0.5f, 0.5f);
        windowRect.anchorMax = new Vector2(0.5f, 0.5f);
        windowRect.sizeDelta = new Vector2(720f, 440f);
        windowRect.anchoredPosition = Vector2.zero;

        Image windowImage = window.GetComponent<Image>();
        windowImage.color = new Color(0.12f, 0.15f, 0.18f, 0.96f);

        TMP_Text title = CreateText("SettingsTitle", window.transform, 30, TextAlignmentOptions.Center);
        SetupLocalizedText(title, "settings.title");
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -38f);
        titleRect.sizeDelta = new Vector2(420f, 50f);

        TMP_Text volumeLabel = CreateText("VolumeLabel", window.transform, 24, TextAlignmentOptions.MidlineLeft);
        SetupLocalizedText(volumeLabel, "settings.volume");
        SetFieldRect(volumeLabel.rectTransform, new Vector2(150f, -110f), new Vector2(220f, 40f));

        GameObject volumeSlider = DefaultControls.CreateSlider(new DefaultControls.Resources());
        volumeSlider.name = "VolumeSlider";
        volumeSlider.transform.SetParent(window.transform, false);
        SetFieldRect(volumeSlider.GetComponent<RectTransform>(), new Vector2(150f, -160f), new Vector2(420f, 40f));

        TMP_Text languageLabel = CreateText("LanguageLabel", window.transform, 24, TextAlignmentOptions.MidlineLeft);
        SetupLocalizedText(languageLabel, "settings.language");
        SetFieldRect(languageLabel.rectTransform, new Vector2(150f, -215f), new Vector2(220f, 40f));

        GameObject languageDropdown = DefaultControls.CreateDropdown(new DefaultControls.Resources());
        languageDropdown.name = "LanguageDropdown";
        languageDropdown.transform.SetParent(window.transform, false);
        SetFieldRect(languageDropdown.GetComponent<RectTransform>(), new Vector2(150f, -265f), new Vector2(420f, 40f));

        TMP_Text fullscreenLabel = CreateText("FullscreenLabel", window.transform, 24, TextAlignmentOptions.MidlineLeft);
        SetupLocalizedText(fullscreenLabel, "settings.fullscreen");
        SetFieldRect(fullscreenLabel.rectTransform, new Vector2(150f, -320f), new Vector2(220f, 40f));

        GameObject fullscreenToggle = DefaultControls.CreateToggle(new DefaultControls.Resources());
        fullscreenToggle.name = "FullscreenToggle";
        fullscreenToggle.transform.SetParent(window.transform, false);
        SetFieldRect(fullscreenToggle.GetComponent<RectTransform>(), new Vector2(445f, -320f), new Vector2(40f, 40f));
        Text toggleLabel = fullscreenToggle.GetComponentInChildren<Text>(true);
        if (toggleLabel != null)
        {
            toggleLabel.text = string.Empty;
        }

        TMP_Text qualityLabel = CreateText("QualityLabel", window.transform, 24, TextAlignmentOptions.MidlineLeft);
        SetupLocalizedText(qualityLabel, "settings.quality");
        SetFieldRect(qualityLabel.rectTransform, new Vector2(150f, -375f), new Vector2(220f, 40f));

        GameObject qualityDropdown = DefaultControls.CreateDropdown(new DefaultControls.Resources());
        qualityDropdown.name = "QualityDropdown";
        qualityDropdown.transform.SetParent(window.transform, false);
        SetFieldRect(qualityDropdown.GetComponent<RectTransform>(), new Vector2(150f, -425f), new Vector2(420f, 40f));

        GameObject closeButton = Object.Instantiate(buttonTemplate, window.transform);
        closeButton.name = "CloseSettingsButton";
        ConfigureMainMenuButton(closeButton, new Vector2(0f, 38f), new Color(0.9f, 0.9f, 0.9f, 1f));
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.5f, 0f);
        closeRect.anchorMax = new Vector2(0.5f, 0f);
        closeRect.anchoredPosition = new Vector2(0f, 42f);
        closeRect.sizeDelta = new Vector2(260f, 56f);
        SetupLocalizedText(closeButton.GetComponentInChildren<TMP_Text>(true), "settings.close");

        Button closeButtonComponent = closeButton.GetComponent<Button>();
        closeButtonComponent.onClick = new Button.ButtonClickedEvent();
        UnityEventTools.AddPersistentListener(closeButtonComponent.onClick, controller.ClosePanel);

        return panelRoot;
    }

    private static void AssignSettingsControllerReferences(SettingsMenuController controller, GameObject settingsPanel)
    {
        SerializedObject serializedController = new SerializedObject(controller);
        serializedController.FindProperty("panelRoot").objectReferenceValue = settingsPanel;
        serializedController.FindProperty("volumeSlider").objectReferenceValue = FindChildRecursive(settingsPanel.transform, "VolumeSlider")?.GetComponent<Slider>();
        serializedController.FindProperty("languageDropdown").objectReferenceValue = FindChildRecursive(settingsPanel.transform, "LanguageDropdown")?.GetComponent<Dropdown>();
        serializedController.FindProperty("fullscreenToggle").objectReferenceValue = FindChildRecursive(settingsPanel.transform, "FullscreenToggle")?.GetComponent<Toggle>();
        serializedController.FindProperty("qualityDropdown").objectReferenceValue = FindChildRecursive(settingsPanel.transform, "QualityDropdown")?.GetComponent<Dropdown>();
        serializedController.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(controller);
    }

    private static void ConfigureMainMenuButton(GameObject buttonObject, Vector2 anchoredPosition, Color imageColor)
    {
        RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(308.434f, 65.9628f);

        Image image = buttonObject.GetComponent<Image>();
        if (image != null)
        {
            image.color = imageColor;
        }
    }

    private static void SetupLocalizedTextByCurrentText(string currentText, string key)
    {
        foreach (TMP_Text text in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (text != null && text.text == currentText)
            {
                SetupLocalizedText(text, key);
            }
        }
    }

    private static void SetupLocalizedText(TMP_Text text, string key)
    {
        if (text == null)
        {
            return;
        }

        LocalizedText localizedText = text.GetComponent<LocalizedText>();
        if (localizedText == null)
        {
            localizedText = text.gameObject.AddComponent<LocalizedText>();
        }

        SerializedObject serializedText = new SerializedObject(localizedText);
        serializedText.FindProperty("localizationKey").stringValue = key;
        serializedText.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(localizedText);
    }

    private static GameObject CreateUiObject(string name, Transform parent, params System.Type[] componentTypes)
    {
        GameObject gameObject = new GameObject(name, componentTypes);
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = gameObject.AddComponent<RectTransform>();
        }

        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    private static TMP_Text CreateText(string name, Transform parent, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject textObject = CreateUiObject(name, parent, typeof(TextMeshProUGUI));
        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.text = name;
        return text;
    }

    private static void SetFieldRect(RectTransform rectTransform, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;
    }

    private static Transform FindChildRecursive(Transform root, string childName)
    {
        if (root == null)
        {
            return null;
        }

        if (root.name == childName)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindChildRecursive(root.GetChild(i), childName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
