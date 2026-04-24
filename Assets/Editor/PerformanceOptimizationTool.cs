using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class PerformanceOptimizationTool
{
    private static readonly string[] PrefabAuditRoots =
    {
        "Assets/Prefabs/Segment.prefab",
        "Assets/Prefabs/StartSegment.prefab",
        "Assets/Prefabs/Segment (1).prefab",
        "Assets/Prefabs/Segment (2).prefab",
        "Assets/Prefabs/Obstacles",
        "Assets/Prefabs/Collectibles",
        "Assets/Prefabs/Decorations"
    };

    private static readonly string[] OptimizedAssetRoots =
    {
        "Assets/Prefabs/Obstacles",
        "Assets/Prefabs/Collectibles",
        "Assets/Prefabs/Decorations",
        "Assets/Prefabs/Resources",
        "Assets/Characters/Nippa"
    };

    [MenuItem("Tools/Performance/Audit Runner Prefabs")]
    public static void AuditRunnerPrefabs()
    {
        StringBuilder report = new StringBuilder();
        report.AppendLine("Runner Performance Audit");
        report.AppendLine("========================");

        foreach (string prefabPath in EnumeratePrefabPaths())
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                continue;
            }

            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
            Light[] lights = prefab.GetComponentsInChildren<Light>(true);
            Collider[] colliders = prefab.GetComponentsInChildren<Collider>(true);
            string[] dependencies = AssetDatabase.GetDependencies(prefabPath, true);
            long modelBytes = 0;
            long textureBytes = 0;

            foreach (string dependency in dependencies)
            {
                string extension = Path.GetExtension(dependency).ToLowerInvariant();
                long bytes = GetFileSize(dependency);
                if (extension == ".fbx")
                {
                    modelBytes += bytes;
                }
                else if (extension == ".png" || extension == ".jpg" || extension == ".jpeg" || extension == ".tga")
                {
                    textureBytes += bytes;
                }
            }

            report.AppendLine();
            report.AppendLine(prefabPath);
            report.AppendLine($"  Renderers: {renderers.Length}");
            report.AppendLine($"  Lights: {lights.Length}");
            report.AppendLine($"  Colliders: {colliders.Length}");
            report.AppendLine($"  Referenced FBX size: {FormatBytes(modelBytes)}");
            report.AppendLine($"  Referenced texture size: {FormatBytes(textureBytes)}");
            report.AppendLine($"  Risk: {EstimateRisk(renderers.Length, lights.Length, colliders.Length, modelBytes, textureBytes)}");
        }

        Debug.Log(report.ToString());
    }

    [MenuItem("Tools/Performance/Apply Balanced Import Settings")]
    public static void ApplyBalancedImportSettings()
    {
        int modelCount = 0;
        int textureCount = 0;

        foreach (string path in EnumerateAssetPaths("t:Model"))
        {
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
            {
                continue;
            }

            importer.isReadable = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.importVisibility = false;
            importer.meshCompression = IsCharacterAsset(path)
                ? ModelImporterMeshCompression.Off
                : ModelImporterMeshCompression.High;
            importer.SaveAndReimport();
            modelCount += 1;
        }

        foreach (string path in EnumerateAssetPaths("t:Texture2D"))
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                continue;
            }

            bool isDataTexture = IsDataTexture(path);
            int maxSize = isDataTexture ? 512 : 1024;
            importer.maxTextureSize = maxSize;
            importer.mipmapEnabled = true;
            importer.sRGBTexture = !isDataTexture;
            importer.textureCompression = TextureImporterCompression.Compressed;
            ApplyPlatformTextureSettings(importer, "Standalone", maxSize);
            ApplyPlatformTextureSettings(importer, "Android", maxSize);
            ApplyPlatformTextureSettings(importer, "iPhone", maxSize);
            importer.SaveAndReimport();
            textureCount += 1;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Applied balanced import settings to {modelCount} model assets and {textureCount} texture assets.");
    }

    private static IEnumerable<string> EnumeratePrefabPaths()
    {
        HashSet<string> paths = new HashSet<string>();

        foreach (string root in PrefabAuditRoots)
        {
            if (root.EndsWith(".prefab"))
            {
                paths.Add(root);
                continue;
            }

            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { root }))
            {
                paths.Add(AssetDatabase.GUIDToAssetPath(guid));
            }
        }

        return paths;
    }

    private static IEnumerable<string> EnumerateAssetPaths(string filter)
    {
        foreach (string guid in AssetDatabase.FindAssets(filter, OptimizedAssetRoots))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.Contains("/TextMesh Pro/"))
            {
                yield return path;
            }
        }
    }

    private static void ApplyPlatformTextureSettings(TextureImporter importer, string platform, int maxSize)
    {
        TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings(platform);
        settings.overridden = true;
        settings.maxTextureSize = maxSize;
        settings.format = TextureImporterFormat.Automatic;
        settings.textureCompression = TextureImporterCompression.Compressed;
        importer.SetPlatformTextureSettings(settings);
    }

    private static bool IsDataTexture(string path)
    {
        string lowerPath = path.ToLowerInvariant();
        return lowerPath.Contains("normal")
            || lowerPath.Contains("_n.")
            || lowerPath.Contains("roughness")
            || lowerPath.Contains("metallic");
    }

    private static bool IsCharacterAsset(string path)
    {
        return path.Replace('\\', '/').StartsWith("Assets/Characters/");
    }

    private static long GetFileSize(string assetPath)
    {
        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
        return File.Exists(fullPath) ? new FileInfo(fullPath).Length : 0;
    }

    private static string EstimateRisk(int rendererCount, int lightCount, int colliderCount, long modelBytes, long textureBytes)
    {
        int score = 0;
        if (lightCount > 4) score += 2;
        if (rendererCount > 12) score += 1;
        if (colliderCount > 12) score += 1;
        if (modelBytes > 50L * 1024L * 1024L) score += 2;
        if (textureBytes > 25L * 1024L * 1024L) score += 1;

        if (score >= 4)
        {
            return "High";
        }

        return score >= 2 ? "Medium" : "Low";
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes <= 0)
        {
            return "0 B";
        }

        if (bytes >= 1024L * 1024L)
        {
            return $"{bytes / (1024f * 1024f):0.0} MB";
        }

        return $"{bytes / 1024f:0.0} KB";
    }
}
