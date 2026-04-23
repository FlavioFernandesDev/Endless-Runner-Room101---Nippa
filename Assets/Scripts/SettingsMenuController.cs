using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class SettingsMenuController : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Dropdown languageDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Dropdown qualityDropdown;

    private bool _isUpdatingUi;
    private bool _isInitialized;

    private void Awake()
    {
        SettingsManager.LoadAndApply();
    }

    public void OpenPanel()
    {
        if (!_isInitialized)
        {
            return;
        }

        RefreshUi();
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }
    }

    public void ClosePanel()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    public void Initialize(MainMenuControl menuControl)
    {
        if (_isInitialized)
        {
            return;
        }

        if (!EnsureUi(menuControl))
        {
            return;
        }

        ConfigureUi();
        RefreshUi();
        ClosePanel();
        _isInitialized = true;
    }

    private void ConfigureUi()
    {
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(HandleVolumeChanged);
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.onValueChanged.AddListener(HandleVolumeChanged);
        }

        if (languageDropdown != null)
        {
            languageDropdown.onValueChanged.RemoveListener(HandleLanguageChanged);
            languageDropdown.ClearOptions();
            languageDropdown.AddOptions(new List<string> { "English", "Portugues" });
            languageDropdown.onValueChanged.AddListener(HandleLanguageChanged);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.RemoveListener(HandleFullscreenChanged);
            fullscreenToggle.onValueChanged.AddListener(HandleFullscreenChanged);
        }

        if (qualityDropdown != null)
        {
            qualityDropdown.onValueChanged.RemoveListener(HandleQualityChanged);
            qualityDropdown.ClearOptions();

            List<string> qualityOptions = new List<string>();
            foreach (string qualityName in QualitySettings.names)
            {
                qualityOptions.Add(qualityName);
            }

            qualityDropdown.AddOptions(qualityOptions);
            qualityDropdown.onValueChanged.AddListener(HandleQualityChanged);
        }
    }

    private bool EnsureUi(MainMenuControl menuControl)
    {
        Transform menuRoot = menuControl != null && menuControl.painelBotoesPrincipais != null
            ? menuControl.painelBotoesPrincipais.transform
            : null;
        if (menuRoot == null)
        {
            return false;
        }

        Button startButton = FindChildComponent<Button>(menuRoot, "StartGame");
        if (startButton == null)
        {
            return false;
        }

        TMP_Text startLabel = startButton.GetComponentInChildren<TMP_Text>(true);
        if (startLabel != null)
        {
            EnsureLocalized(startLabel, "menu.start");
        }

        startButton.onClick = new Button.ButtonClickedEvent();
        startButton.onClick.AddListener(menuControl.StartGame);

        Button quitButton = FindChildComponent<Button>(menuRoot, "BotãoSair");
        if (quitButton != null)
        {
            TMP_Text quitLabel = quitButton.GetComponentInChildren<TMP_Text>(true);
            if (quitLabel != null)
            {
                EnsureLocalized(quitLabel, "menu.quit");
            }
        }

        if (menuControl != null && menuControl.mensagemCliqueInicial != null)
        {
            TMP_Text promptText = menuControl.mensagemCliqueInicial.GetComponent<TMP_Text>();
            if (promptText != null)
            {
                EnsureLocalized(promptText, "menu.game_over_prompt");
            }
        }

        Button settingsButton = FindChildComponent<Button>(menuRoot, "SettingsButton");
        if (settingsButton == null)
        {
            settingsButton = Instantiate(startButton, menuRoot);
            settingsButton.name = "SettingsButton";
            RectTransform settingsRect = settingsButton.GetComponent<RectTransform>();
            RectTransform startRect = startButton.GetComponent<RectTransform>();
            settingsRect.anchoredPosition = startRect.anchoredPosition + new Vector2(0f, -95f);

            Image settingsImage = settingsButton.GetComponent<Image>();
            if (settingsImage != null)
            {
                settingsImage.color = new Color(0.58f, 0.84f, 1f, 1f);
            }
        }

        settingsButton.onClick = new Button.ButtonClickedEvent();
        settingsButton.onClick.AddListener(OpenPanel);

        TMP_Text settingsLabel = settingsButton.GetComponentInChildren<TMP_Text>(true);
        if (settingsLabel != null)
        {
            ConfigureMainMenuButtonLabel(settingsLabel, 48f, 30f);
            EnsureLocalized(settingsLabel, "menu.settings");
        }

        Button achievementsButton = FindChildComponent<Button>(menuRoot, "AchievementsButton");
        if (achievementsButton == null)
        {
            achievementsButton = Instantiate(startButton, menuRoot);
            achievementsButton.name = "AchievementsButton";
            RectTransform achievementsRect = achievementsButton.GetComponent<RectTransform>();
            RectTransform startRect = startButton.GetComponent<RectTransform>();
            achievementsRect.anchoredPosition = startRect.anchoredPosition + new Vector2(0f, -190f);

            Image achievementsImage = achievementsButton.GetComponent<Image>();
            if (achievementsImage != null)
            {
                achievementsImage.color = new Color(1f, 0.84f, 0.48f, 1f);
            }
        }

        achievementsButton.onClick = new Button.ButtonClickedEvent();
        achievementsButton.onClick.AddListener(menuControl.OpenAchievements);

        TMP_Text achievementsLabel = achievementsButton.GetComponentInChildren<TMP_Text>(true);
        if (achievementsLabel != null)
        {
            ConfigureMainMenuButtonLabel(achievementsLabel, 44f, 26f);
            EnsureLocalized(achievementsLabel, "menu.achievements");
        }

        if (panelRoot == null)
        {
            Canvas canvas = FindAnyObjectByType<Canvas>(FindObjectsInactive.Include);
            if (canvas != null)
            {
                panelRoot = CreateSettingsPanel(canvas.transform, startButton);
            }
        }

        return true;
    }

    private void RefreshUi()
    {
        _isUpdatingUi = true;
        SettingsManager.LoadAndApply();

        if (volumeSlider != null)
        {
            volumeSlider.SetValueWithoutNotify(SettingsManager.MasterVolume);
        }

        if (languageDropdown != null)
        {
            languageDropdown.SetValueWithoutNotify((int)SettingsManager.Language);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.SetIsOnWithoutNotify(SettingsManager.Fullscreen);
        }

        if (qualityDropdown != null)
        {
            qualityDropdown.SetValueWithoutNotify(SettingsManager.QualityLevel);
        }

        _isUpdatingUi = false;
    }

    private GameObject CreateSettingsPanel(Transform canvasTransform, Button buttonTemplate)
    {
        GameObject overlayObject = CreateUiObject("SettingsPanel", canvasTransform, typeof(Image));
        RectTransform overlayRect = overlayObject.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        Image overlayImage = overlayObject.GetComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.68f);

        GameObject windowObject = CreateUiObject("Window", overlayObject.transform, typeof(Image));
        RectTransform windowRect = windowObject.GetComponent<RectTransform>();
        windowRect.anchorMin = new Vector2(0.5f, 0.5f);
        windowRect.anchorMax = new Vector2(0.5f, 0.5f);
        windowRect.pivot = new Vector2(0.5f, 0.5f);
        windowRect.sizeDelta = new Vector2(720f, 520f);
        windowRect.anchoredPosition = Vector2.zero;

        Image windowImage = windowObject.GetComponent<Image>();
        windowImage.color = new Color(0.11f, 0.14f, 0.17f, 0.98f);

        TMP_Text templateText = buttonTemplate.GetComponentInChildren<TMP_Text>(true);
        TMP_Text title = CreateText("SettingsTitle", "Settings", windowObject.transform, templateText, 30f, TextAlignmentOptions.Center);
        EnsureLocalized(title, "settings.title");
        SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -38f), new Vector2(420f, 48f), new Vector2(0.5f, 0.5f));

        TMP_Text volumeLabel = CreateText("VolumeLabel", "Volume", windowObject.transform, templateText, 24f, TextAlignmentOptions.Left);
        EnsureLocalized(volumeLabel, "settings.volume");
        SetRect(volumeLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(80f, -110f), new Vector2(220f, 36f), new Vector2(0f, 0.5f));

        GameObject volumeSliderObject = DefaultControls.CreateSlider(new DefaultControls.Resources());
        volumeSliderObject.name = "VolumeSlider";
        volumeSliderObject.transform.SetParent(windowObject.transform, false);
        volumeSlider = volumeSliderObject.GetComponent<Slider>();
        SetRect(volumeSliderObject.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(80f, -160f), new Vector2(560f, 30f), new Vector2(0f, 0.5f));

        TMP_Text languageLabel = CreateText("LanguageLabel", "Language", windowObject.transform, templateText, 24f, TextAlignmentOptions.Left);
        EnsureLocalized(languageLabel, "settings.language");
        SetRect(languageLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(80f, -215f), new Vector2(220f, 36f), new Vector2(0f, 0.5f));

        GameObject languageDropdownObject = DefaultControls.CreateDropdown(new DefaultControls.Resources());
        languageDropdownObject.name = "LanguageDropdown";
        languageDropdownObject.transform.SetParent(windowObject.transform, false);
        languageDropdown = languageDropdownObject.GetComponent<Dropdown>();
        SetRect(languageDropdownObject.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(80f, -265f), new Vector2(560f, 40f), new Vector2(0f, 0.5f));

        TMP_Text fullscreenLabel = CreateText("FullscreenLabel", "Fullscreen", windowObject.transform, templateText, 24f, TextAlignmentOptions.Left);
        EnsureLocalized(fullscreenLabel, "settings.fullscreen");
        SetRect(fullscreenLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(80f, -320f), new Vector2(240f, 36f), new Vector2(0f, 0.5f));

        GameObject fullscreenToggleObject = DefaultControls.CreateToggle(new DefaultControls.Resources());
        fullscreenToggleObject.name = "FullscreenToggle";
        fullscreenToggleObject.transform.SetParent(windowObject.transform, false);
        fullscreenToggle = fullscreenToggleObject.GetComponent<Toggle>();
        SetRect(fullscreenToggleObject.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(560f, -320f), new Vector2(40f, 40f), new Vector2(0f, 0.5f));
        Text toggleText = fullscreenToggleObject.GetComponentInChildren<Text>(true);
        if (toggleText != null)
        {
            toggleText.text = string.Empty;
        }

        TMP_Text qualityLabel = CreateText("QualityLabel", "Quality", windowObject.transform, templateText, 24f, TextAlignmentOptions.Left);
        EnsureLocalized(qualityLabel, "settings.quality");
        SetRect(qualityLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(80f, -385f), new Vector2(220f, 36f), new Vector2(0f, 0.5f));

        GameObject qualityDropdownObject = DefaultControls.CreateDropdown(new DefaultControls.Resources());
        qualityDropdownObject.name = "QualityDropdown";
        qualityDropdownObject.transform.SetParent(windowObject.transform, false);
        qualityDropdown = qualityDropdownObject.GetComponent<Dropdown>();
        SetRect(qualityDropdownObject.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(80f, -435f), new Vector2(560f, 40f), new Vector2(0f, 0.5f));

        Button closeButton = Instantiate(buttonTemplate, windowObject.transform);
        closeButton.name = "CloseSettingsButton";
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.5f, 0f);
        closeRect.anchorMax = new Vector2(0.5f, 0f);
        closeRect.pivot = new Vector2(0.5f, 0.5f);
        closeRect.anchoredPosition = new Vector2(0f, 46f);
        closeRect.sizeDelta = new Vector2(260f, 56f);
        Image closeImage = closeButton.GetComponent<Image>();
        if (closeImage != null)
        {
            closeImage.color = new Color(0.92f, 0.92f, 0.92f, 1f);
        }

        closeButton.onClick = new Button.ButtonClickedEvent();
        closeButton.onClick.AddListener(ClosePanel);
        TMP_Text closeLabel = closeButton.GetComponentInChildren<TMP_Text>(true);
        if (closeLabel != null)
        {
            EnsureLocalized(closeLabel, "settings.close");
        }

        return overlayObject;
    }

    private static GameObject CreateUiObject(string name, Transform parent, params System.Type[] componentTypes)
    {
        GameObject gameObject = new GameObject(name, componentTypes);
        gameObject.transform.SetParent(parent, false);
        if (gameObject.GetComponent<RectTransform>() == null)
        {
            gameObject.AddComponent<RectTransform>();
        }

        return gameObject;
    }

    private static TMP_Text CreateText(string name, string textValue, Transform parent, TMP_Text template, float fontSize, TextAlignmentOptions alignment)
    {
        TMP_Text text;
        if (template != null)
        {
            text = Instantiate(template, parent);
            text.name = name;
        }
        else
        {
            GameObject textObject = CreateUiObject(name, parent, typeof(TextMeshProUGUI));
            text = textObject.GetComponent<TMP_Text>();
        }

        text.text = textValue;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        return text;
    }

    private static void ConfigureMainMenuButtonLabel(TMP_Text label, float maxFontSize, float minFontSize)
    {
        label.enableAutoSizing = true;
        label.fontSizeMax = maxFontSize;
        label.fontSizeMin = minFontSize;
        label.fontSize = maxFontSize;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.overflowMode = TextOverflowModes.Truncate;
        label.alignment = TextAlignmentOptions.Center;

        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(18f, 0f);
        labelRect.offsetMax = new Vector2(-18f, 0f);
    }

    private static void SetRect(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta, Vector2 pivot)
    {
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot;
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;
    }

    private static T FindChildComponent<T>(Transform root, string childName) where T : Component
    {
        Transform child = FindChildRecursive(root, childName);
        return child != null ? child.GetComponent<T>() : null;
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

    private static void EnsureLocalized(TMP_Text text, string key)
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

        localizedText.AssignKey(key);
    }

    private void HandleVolumeChanged(float value)
    {
        if (_isUpdatingUi)
        {
            return;
        }

        SettingsManager.SetMasterVolume(value);
    }

    private void HandleLanguageChanged(int value)
    {
        if (_isUpdatingUi)
        {
            return;
        }

        SettingsManager.SetLanguage((AppLanguage)Mathf.Clamp(value, 0, 1));
    }

    private void HandleFullscreenChanged(bool value)
    {
        if (_isUpdatingUi)
        {
            return;
        }

        SettingsManager.SetFullscreen(value);
    }

    private void HandleQualityChanged(int value)
    {
        if (_isUpdatingUi)
        {
            return;
        }

        SettingsManager.SetQuality(value);
    }
}
