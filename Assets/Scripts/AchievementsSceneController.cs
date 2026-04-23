using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class AchievementsSceneController : MonoBehaviour
{
    private static readonly Color PageBackground = new(0.035f, 0.055f, 0.07f, 0.98f);
    private static readonly Color PanelBackground = new(0.075f, 0.105f, 0.125f, 0.96f);
    private static readonly Color CardLocked = new(0.13f, 0.15f, 0.17f, 1f);
    private static readonly Color CardUnlocked = new(0.12f, 0.22f, 0.18f, 1f);
    private static readonly Color MutedText = new(0.76f, 0.84f, 0.86f, 0.92f);

    private GameObject _root;

    public void Initialize(Scene scene)
    {
        if (_root != null)
        {
            RefreshLocalizedContent();
            return;
        }

        Canvas canvas = FindCanvasInScene(scene);
        if (canvas == null)
        {
            return;
        }

        HideExistingCanvasChildren(canvas.transform);
        _root = BuildUi(canvas.transform);
    }

    public void RefreshLocalizedContent()
    {
        if (_root == null)
        {
            return;
        }

        Transform canvasTransform = _root.transform.parent;
        Destroy(_root);
        _root = BuildUi(canvasTransform);
    }

    private GameObject BuildUi(Transform canvasTransform)
    {
        GameObject pageRoot = CreateUiObject("AchievementsPage", canvasTransform, typeof(Image));
        SetStretch(pageRoot.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
        pageRoot.GetComponent<Image>().color = PageBackground;

        GameObject headerBand = CreatePanel("HeaderBand", pageRoot.transform, new Color(0.07f, 0.16f, 0.18f, 0.72f));
        SetStretch(headerBand.GetComponent<RectTransform>(), new Vector2(0f, 760f), Vector2.zero);

        TMP_Text title = CreateText("AchievementsTitle", pageRoot.transform, 46f, TextAlignmentOptions.Center);
        SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -56f), new Vector2(620f, 58f), new Vector2(0.5f, 0.5f));
        EnsureLocalized(title, "ach.page.title");

        TMP_Text subtitle = CreateText("AchievementsSubtitle", pageRoot.transform, 20f, TextAlignmentOptions.Center);
        subtitle.color = MutedText;
        subtitle.textWrappingMode = TextWrappingModes.NoWrap;
        SetRect(subtitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -104f), new Vector2(920f, 34f), new Vector2(0.5f, 0.5f));
        EnsureLocalized(subtitle, "ach.page.subtitle");

        GameObject summaryPanel = CreatePanel("SummaryPanel", pageRoot.transform, new Color(0.055f, 0.07f, 0.085f, 0.92f));
        SetRect(summaryPanel.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -186f), new Vector2(1140f, 124f), new Vector2(0.5f, 0.5f));

        int totalAchievements = AchievementsManager.GetAllAchievements().Count;
        int unlockedAchievements = AchievementsManager.GetUnlockedCount();
        CreateSummaryTile(summaryPanel.transform, "SummaryUnlocked", "ach.summary.unlocked", FormatLocalized("ach.summary.unlocked_value", unlockedAchievements, totalAchievements), -405f, new Color(0.38f, 0.95f, 0.67f, 1f));
        CreateSummaryTile(summaryPanel.transform, "SummaryBestRun", "ach.summary.best_run", $"{AchievementsManager.GetBestRunDistance()}m", -135f, GetCategoryColor(AchievementProgressType.BestRunDistance));
        CreateSummaryTile(summaryPanel.transform, "SummaryCoins", "ach.summary.coins", RunManager.Instance.TotalCoins.ToString(), 135f, GetCategoryColor(AchievementProgressType.TotalCoins));
        CreateSummaryTile(summaryPanel.transform, "SummaryKeys", "ach.summary.keys", RunManager.Instance.TotalKeys.ToString(), 405f, GetCategoryColor(AchievementProgressType.TotalKeys));

        GameObject cardsPanel = CreatePanel("CardsPanel", pageRoot.transform, PanelBackground);
        RectTransform cardsRect = cardsPanel.GetComponent<RectTransform>();
        cardsRect.anchorMin = new Vector2(0.5f, 0f);
        cardsRect.anchorMax = new Vector2(0.5f, 1f);
        cardsRect.pivot = new Vector2(0.5f, 0.5f);
        cardsRect.anchoredPosition = new Vector2(0f, -74f);
        cardsRect.sizeDelta = new Vector2(1160f, -390f);

        GameObject scrollView = CreateScrollView(cardsPanel.transform);
        SetStretch(scrollView.GetComponent<RectTransform>(), new Vector2(28f, 28f), new Vector2(-28f, -28f));

        Transform content = scrollView.transform.Find("Viewport/Content");
        VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 16f;
        layout.padding = new RectOffset(8, 8, 8, 8);
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        foreach (AchievementDefinition achievement in AchievementsManager.GetAllAchievements())
        {
            CreateAchievementCard(content, achievement);
        }

        Button backButton = CreateActionButton(pageRoot.transform, "BackButton", new Color(0.92f, 0.94f, 0.94f, 1f), new Vector2(0f, 54f));
        backButton.onClick = new Button.ButtonClickedEvent();
        backButton.onClick.AddListener(() =>
        {
            MainMenuControl.saltarIntro = true;
            SceneManager.LoadScene(RunManager.MainMenuSceneName);
        });
        TMP_Text backText = backButton.GetComponentInChildren<TMP_Text>(true);
        EnsureLocalized(backText, "ach.page.back");

        return pageRoot;
    }

    private void CreateSummaryTile(Transform parent, string name, string labelKey, string value, float xOffset, Color accent)
    {
        GameObject tile = CreatePanel(name, parent, new Color(0.105f, 0.135f, 0.155f, 1f));
        SetRect(tile.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(xOffset, 0f), new Vector2(240f, 86f), new Vector2(0.5f, 0.5f));

        GameObject accentBar = CreatePanel(name + "Accent", tile.transform, accent);
        SetStretch(accentBar.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(-232f, 0f));

        TMP_Text label = CreateText(name + "Label", tile.transform, 17f, TextAlignmentOptions.Center);
        label.color = MutedText;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        SetRect(label.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -26f), new Vector2(200f, 26f), new Vector2(0.5f, 0.5f));
        EnsureLocalized(label, labelKey);

        TMP_Text amount = CreateText(name + "Value", tile.transform, 30f, TextAlignmentOptions.Center);
        amount.color = Color.white;
        amount.textWrappingMode = TextWrappingModes.NoWrap;
        SetRect(amount.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 28f), new Vector2(200f, 34f), new Vector2(0.5f, 0.5f));
        amount.text = value;
    }

    private void CreateAchievementCard(Transform parent, AchievementDefinition achievement)
    {
        bool unlocked = AchievementsManager.IsUnlocked(achievement.Id);
        int progress = AchievementsManager.GetProgress(achievement);
        float percent = AchievementsManager.GetProgressPercent(achievement);
        Color accent = GetCategoryColor(achievement.ProgressType);

        GameObject card = CreatePanel(achievement.Id, parent, unlocked ? CardUnlocked : CardLocked);
        LayoutElement layoutElement = card.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 146f;
        layoutElement.minHeight = 146f;

        RectTransform rect = card.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0f, 146f);

        GameObject typeBadge = CreatePanel(achievement.Id + "_TypeBadge", card.transform, new Color(accent.r, accent.g, accent.b, unlocked ? 0.34f : 0.22f));
        SetRect(typeBadge.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(34f, -31f), new Vector2(38f, 38f), new Vector2(0.5f, 0.5f));

        TMP_Text typeBadgeText = CreateText(achievement.Id + "_TypeBadgeText", typeBadge.transform, 18f, TextAlignmentOptions.Center);
        typeBadgeText.color = Color.white;
        typeBadgeText.textWrappingMode = TextWrappingModes.NoWrap;
        typeBadgeText.text = GetCategoryBadgeText(achievement.ProgressType);
        SetStretch(typeBadgeText.rectTransform, Vector2.zero, Vector2.zero);

        GameObject categoryPill = CreatePanel(achievement.Id + "_CategoryPill", card.transform, new Color(accent.r, accent.g, accent.b, 0.18f));
        SetRect(categoryPill.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(116f, -30f), new Vector2(140f, 30f), new Vector2(0.5f, 0.5f));

        TMP_Text category = CreateText(achievement.Id + "_Category", categoryPill.transform, 15f, TextAlignmentOptions.Center);
        category.color = accent;
        category.textWrappingMode = TextWrappingModes.NoWrap;
        SetStretch(category.rectTransform, new Vector2(8f, 0f), new Vector2(-8f, 0f));
        EnsureLocalized(category, AchievementsManager.GetCategoryKey(achievement.ProgressType));

        TMP_Text title = CreateText(achievement.Id + "_Title", card.transform, 25f, TextAlignmentOptions.Left);
        title.textWrappingMode = TextWrappingModes.NoWrap;
        title.overflowMode = TextOverflowModes.Truncate;
        SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(206f, -31f), new Vector2(-434f, 32f), new Vector2(0f, 0.5f));
        EnsureLocalized(title, achievement.TitleKey);

        TMP_Text status = CreateText(achievement.Id + "_Status", card.transform, 16f, TextAlignmentOptions.Center);
        status.color = unlocked ? new Color(0.72f, 1f, 0.82f, 1f) : new Color(0.85f, 0.9f, 0.92f, 0.9f);
        status.textWrappingMode = TextWrappingModes.NoWrap;
        SetRect(status.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-106f, -31f), new Vector2(170f, 30f), new Vector2(0.5f, 0.5f));
        status.text = LocalizationTable.Get(unlocked ? "ach.status.unlocked" : "ach.status.locked", SettingsManager.Language);

        TMP_Text description = CreateText(achievement.Id + "_Description", card.transform, 18f, TextAlignmentOptions.Left);
        description.color = MutedText;
        description.textWrappingMode = TextWrappingModes.Normal;
        SetRect(description.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(32f, -70f), new Vector2(-364f, 42f), new Vector2(0f, 0.5f));
        EnsureLocalized(description, achievement.DescriptionKey);

        TMP_Text progressLabel = CreateText(achievement.Id + "_ProgressLabel", card.transform, 16f, TextAlignmentOptions.Left);
        progressLabel.color = new Color(0.78f, 0.86f, 0.88f, 0.9f);
        progressLabel.textWrappingMode = TextWrappingModes.NoWrap;
        SetRect(progressLabel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(32f, 43f), new Vector2(180f, 24f), new Vector2(0f, 0.5f));
        EnsureLocalized(progressLabel, "ach.progress.label");

        TMP_Text progressText = CreateText(achievement.Id + "_Progress", card.transform, 18f, TextAlignmentOptions.Right);
        progressText.color = Color.white;
        progressText.textWrappingMode = TextWrappingModes.NoWrap;
        SetRect(progressText.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-34f, 43f), new Vector2(260f, 26f), new Vector2(1f, 0.5f));
        progressText.text = FormatLocalized(AchievementsManager.GetProgressFormatKey(achievement.ProgressType), progress, achievement.TargetValue);

        GameObject barBackground = CreatePanel(achievement.Id + "_BarBg", card.transform, new Color(0.035f, 0.045f, 0.055f, 0.9f));
        RectTransform barBgRect = barBackground.GetComponent<RectTransform>();
        barBgRect.anchorMin = new Vector2(0f, 0f);
        barBgRect.anchorMax = new Vector2(1f, 0f);
        barBgRect.pivot = new Vector2(0.5f, 0.5f);
        barBgRect.anchoredPosition = new Vector2(0f, 20f);
        barBgRect.offsetMin = new Vector2(32f, barBgRect.offsetMin.y);
        barBgRect.offsetMax = new Vector2(-32f, barBgRect.offsetMax.y);
        barBgRect.sizeDelta = new Vector2(barBgRect.sizeDelta.x, 14f);

        GameObject barFill = CreateUiObject(achievement.Id + "_BarFill", barBackground.transform, typeof(Image));
        RectTransform barFillRect = barFill.GetComponent<RectTransform>();
        barFillRect.anchorMin = Vector2.zero;
        barFillRect.anchorMax = new Vector2(percent, 1f);
        barFillRect.offsetMin = Vector2.zero;
        barFillRect.offsetMax = Vector2.zero;
        barFill.GetComponent<Image>().color = unlocked ? new Color(0.46f, 1f, 0.65f, 1f) : accent;
    }

    private static Canvas FindCanvasInScene(Scene scene)
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include);
        foreach (Canvas canvas in canvases)
        {
            if (canvas != null && canvas.gameObject.scene == scene)
            {
                return canvas;
            }
        }

        return null;
    }

    private static void HideExistingCanvasChildren(Transform canvasTransform)
    {
        for (int i = 0; i < canvasTransform.childCount; i++)
        {
            canvasTransform.GetChild(i).gameObject.SetActive(false);
        }
    }

    private static Button CreateActionButton(Transform parent, string name, Color color, Vector2 anchoredPosition)
    {
        GameObject buttonObject = CreateUiObject(name, parent, typeof(Image), typeof(Button));
        SetRect(buttonObject.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), anchoredPosition, new Vector2(260f, 58f), new Vector2(0.5f, 0.5f));
        buttonObject.GetComponent<Image>().color = color;

        TMP_Text text = CreateText(name + "Text", buttonObject.transform, 24f, TextAlignmentOptions.Center);
        text.color = new Color(0.13f, 0.16f, 0.18f, 1f);
        text.textWrappingMode = TextWrappingModes.NoWrap;
        SetStretch(text.rectTransform, Vector2.zero, Vector2.zero);
        return buttonObject.GetComponent<Button>();
    }

    private static GameObject CreatePanel(string name, Transform parent, Color color)
    {
        GameObject panel = CreateUiObject(name, parent, typeof(Image));
        panel.GetComponent<Image>().color = color;
        return panel;
    }

    private static GameObject CreateScrollView(Transform parent)
    {
        GameObject scrollView = CreateUiObject("Scroll View", parent, typeof(Image), typeof(Mask), typeof(ScrollRect));
        scrollView.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.01f);
        scrollView.GetComponent<Mask>().showMaskGraphic = false;

        GameObject viewport = CreateUiObject("Viewport", scrollView.transform, typeof(Image), typeof(Mask));
        SetStretch(viewport.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
        viewport.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.01f);
        viewport.GetComponent<Mask>().showMaskGraphic = false;

        GameObject content = CreateUiObject("Content", viewport.transform, typeof(RectTransform));
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 0f);

        ScrollRect scrollRect = scrollView.GetComponent<ScrollRect>();
        scrollRect.viewport = viewport.GetComponent<RectTransform>();
        scrollRect.content = contentRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 26f;

        return scrollView;
    }

    private static TMP_Text CreateText(string name, Transform parent, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject textObject = CreateUiObject(name, parent, typeof(TextMeshProUGUI));
        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        return text;
    }

    private static GameObject CreateUiObject(string name, Transform parent, params System.Type[] components)
    {
        GameObject gameObject = new(name, components);
        gameObject.transform.SetParent(parent, false);
        if (gameObject.GetComponent<RectTransform>() == null)
        {
            gameObject.AddComponent<RectTransform>();
        }

        return gameObject;
    }

    private static void EnsureLocalized(TMP_Text text, string key)
    {
        if (text == null)
        {
            return;
        }

        LocalizedText localized = text.GetComponent<LocalizedText>();
        if (localized == null)
        {
            localized = text.gameObject.AddComponent<LocalizedText>();
        }

        localized.AssignKey(key);
    }

    private static string FormatLocalized(string key, params object[] values)
    {
        return string.Format(LocalizationTable.Get(key, SettingsManager.Language), values);
    }

    private static Color GetCategoryColor(AchievementProgressType progressType)
    {
        return progressType switch
        {
            AchievementProgressType.BestRunDistance => new Color(0.36f, 0.82f, 0.95f, 1f),
            AchievementProgressType.TotalCoins => new Color(1f, 0.76f, 0.24f, 1f),
            AchievementProgressType.TotalKeys => new Color(0.42f, 0.94f, 0.58f, 1f),
            _ => new Color(0.78f, 0.86f, 0.88f, 1f)
        };
    }

    private static string GetCategoryBadgeText(AchievementProgressType progressType)
    {
        return progressType switch
        {
            AchievementProgressType.BestRunDistance => "M",
            AchievementProgressType.TotalCoins => "C",
            AchievementProgressType.TotalKeys => "K",
            _ => "+"
        };
    }

    private static void SetStretch(RectTransform rectTransform, Vector2 offsetMin, Vector2 offsetMax)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = offsetMin;
        rectTransform.offsetMax = offsetMax;
    }

    private static void SetRect(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta, Vector2 pivot)
    {
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot;
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;
    }
}
