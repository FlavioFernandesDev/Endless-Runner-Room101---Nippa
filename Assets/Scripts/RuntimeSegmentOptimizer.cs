using UnityEngine;
using UnityEngine.Rendering;

public static class RuntimeSegmentOptimizer
{
    private const int DefaultMaxActivePointLights = 2;

    public static void ApplyToSegment(GameObject segmentRoot, int maxActivePointLights = DefaultMaxActivePointLights)
    {
        if (segmentRoot == null)
        {
            return;
        }

        OptimizeLights(segmentRoot, maxActivePointLights);
        OptimizeRenderers(segmentRoot);
    }

    private static void OptimizeLights(GameObject segmentRoot, int maxActivePointLights)
    {
        Light[] lights = segmentRoot.GetComponentsInChildren<Light>(true);
        int enabledPointLights = 0;
        int pointLightLimit = Mathf.Max(0, maxActivePointLights);

        foreach (Light currentLight in lights)
        {
            if (currentLight == null)
            {
                continue;
            }

            currentLight.shadows = LightShadows.None;

            if (currentLight.type != LightType.Point)
            {
                continue;
            }

            bool keepEnabled = enabledPointLights < pointLightLimit;
            currentLight.enabled = keepEnabled;
            if (keepEnabled)
            {
                enabledPointLights += 1;
            }
        }
    }

    private static void OptimizeRenderers(GameObject segmentRoot)
    {
        Renderer[] renderers = segmentRoot.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer currentRenderer in renderers)
        {
            if (currentRenderer == null)
            {
                continue;
            }

            currentRenderer.shadowCastingMode = ShadowCastingMode.Off;
            currentRenderer.receiveShadows = false;
        }
    }
}
