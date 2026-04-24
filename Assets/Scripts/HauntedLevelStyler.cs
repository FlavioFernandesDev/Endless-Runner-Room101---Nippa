using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class HauntedLevelStyler
{
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        ApplyToScene(SceneManager.GetActiveScene());
    }

    public static bool IsHauntedSceneActive()
    {
        return SceneManager.GetActiveScene().name == RunManager.HauntedGameplaySceneName;
    }

    public static void ApplyTo(GameObject root)
    {
        if (!IsHauntedSceneActive() || root == null)
        {
            return;
        }

        Light[] lights = root.GetComponentsInChildren<Light>(true);
        foreach (Light currentLight in lights)
        {
            ApplyToLight(currentLight);
        }

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer currentRenderer in renderers)
        {
            ApplyToRenderer(currentRenderer);
        }
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        ApplyToScene(scene);
    }

    private static void ApplyToScene(Scene scene)
    {
        if (scene.name != RunManager.HauntedGameplaySceneName)
        {
            return;
        }

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.035f, 0.045f, 0.075f, 1f);
        RenderSettings.fogDensity = 0.018f;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.075f, 0.08f, 0.13f, 1f);
        RenderSettings.ambientIntensity = 0.45f;

        GameObject[] roots = scene.GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            ApplyTo(root);
        }

        Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include);
        foreach (Camera currentCamera in cameras)
        {
            if (currentCamera != null && currentCamera.gameObject.scene == scene)
            {
                currentCamera.backgroundColor = new Color(0.03f, 0.035f, 0.06f, 1f);
            }
        }
    }

    private static void ApplyToLight(Light currentLight)
    {
        if (currentLight == null)
        {
            return;
        }

        currentLight.color = new Color(0.5f, 0.8f, 0.95f, 1f);
        currentLight.intensity = Mathf.Clamp(currentLight.intensity * 0.75f, 0.15f, 1.2f);
        currentLight.range = Mathf.Max(currentLight.range, 8f);
    }

    private static void ApplyToRenderer(Renderer currentRenderer)
    {
        if (currentRenderer == null
            || currentRenderer.GetComponentInParent<Canvas>() != null
            || currentRenderer.GetComponentInParent<PlayerMovement>() != null
            || currentRenderer.GetComponentInParent<CollectCoin>() != null
            || currentRenderer.GetComponentInParent<CollectKey>() != null)
        {
            return;
        }

        Color hauntedColor = ResolveHauntedColor(currentRenderer.gameObject.name);
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        currentRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(BaseColorId, hauntedColor);
        propertyBlock.SetColor(ColorId, hauntedColor);
        currentRenderer.SetPropertyBlock(propertyBlock);
    }

    private static Color ResolveHauntedColor(string objectName)
    {
        string normalizedName = objectName.ToLowerInvariant();
        if (normalizedName.Contains("floor") || normalizedName.Contains("carpet"))
        {
            return new Color(0.16f, 0.045f, 0.085f, 1f);
        }

        if (normalizedName.Contains("wall") || normalizedName.Contains("door"))
        {
            return new Color(0.1f, 0.105f, 0.17f, 1f);
        }

        if (normalizedName.Contains("ceiling") || normalizedName.Contains("celing"))
        {
            return new Color(0.045f, 0.05f, 0.075f, 1f);
        }

        if (normalizedName.Contains("lamp") || normalizedName.Contains("light"))
        {
            return new Color(0.35f, 0.85f, 0.95f, 1f);
        }

        return new Color(0.13f, 0.12f, 0.18f, 1f);
    }
}
