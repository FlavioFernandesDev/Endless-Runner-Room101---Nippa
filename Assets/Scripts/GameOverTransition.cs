using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class GameOverTransition
{
    private const string FadeOutObjectName = "FadeOut";
    private const string GameOverTextName = "GameOverText";

    public static IEnumerator Play(GameObject fadeOut, float delayBeforeFade = 2f, float delayAfterFade = 2f)
    {
        yield return new WaitForSeconds(delayBeforeFade);

        GameObject overlay = fadeOut != null ? fadeOut : FindInactiveObjectInActiveScene(FadeOutObjectName);
        if (overlay != null)
        {
            EnsureGameOverText(overlay.transform);
            overlay.SetActive(true);
        }

        yield return new WaitForSeconds(delayAfterFade);
        SceneManager.LoadScene(RunManager.StageSelectSceneName);
    }

    private static void EnsureGameOverText(Transform fadeOutTransform)
    {
        Transform existingText = fadeOutTransform.Find(GameOverTextName);
        TMP_Text label = existingText != null ? existingText.GetComponent<TMP_Text>() : null;

        if (label == null)
        {
            GameObject textObject = new GameObject(GameOverTextName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(fadeOutTransform, false);
            label = textObject.GetComponent<TextMeshProUGUI>();
        }

        RectTransform rectTransform = label.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        label.text = "GAME OVER";
        label.color = Color.red;
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 86f;
        label.fontStyle = FontStyles.Bold;
        label.raycastTarget = false;

        Image overlayImage = fadeOutTransform.GetComponent<Image>();
        if (overlayImage != null)
        {
            overlayImage.color = Color.black;
        }

        RawImage overlayRawImage = fadeOutTransform.GetComponent<RawImage>();
        if (overlayRawImage != null && overlayRawImage.color.a <= 0f)
        {
            overlayRawImage.color = Color.black;
        }
    }

    private static GameObject FindInactiveObjectInActiveScene(string objectName)
    {
        Scene activeScene = SceneManager.GetActiveScene();
        foreach (GameObject root in activeScene.GetRootGameObjects())
        {
            GameObject match = FindChildRecursive(root.transform, objectName);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }

    private static GameObject FindChildRecursive(Transform parent, string objectName)
    {
        if (parent.name == objectName)
        {
            return parent.gameObject;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            GameObject match = FindChildRecursive(parent.GetChild(i), objectName);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }
}
