using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class SegmentCollectibleSetupTool
{
    private const string GeneratedCollectibleFolder = "Assets/Prefabs/Collectibles/Generated";
    private const string CoinPrefabPath = GeneratedCollectibleFolder + "/CoinPickup.prefab";
    private const string KeyPrefabPath = GeneratedCollectibleFolder + "/KeyPickup.prefab";
    private const string KeyVisualPath = "Assets/Prefabs/Collectibles/Key.fbx";
    private const string GoldCoinMaterialPath = "Assets/Materials/GoldCoin.mat";
    private const string CoinSfxPath = "Assets/Audio/Fx/CoinSFX.wav";
    private static readonly Vector3[] LaneRootLocalPositions =
    {
        new Vector3(-2f, 0.5f, 15f),
        new Vector3(-0.15f, 0.5f, 15f),
        new Vector3(1.7f, 0.5f, 15f)
    };
    private static readonly Vector3 EndNodeRootLocalPosition = new Vector3(0f, 0f, 30f);

    private static readonly string[] SegmentPrefabPaths =
    {
        "Assets/Prefabs/Segment.prefab",
        "Assets/Prefabs/Segment (1).prefab",
        "Assets/Prefabs/Segment (2).prefab",
        "Assets/Prefabs/StartSegment.prefab"
    };

    [MenuItem("Tools/Gameplay/Setup Segment Collectibles")]
    public static void RunBatchSetup()
    {
        EnsureFolders();

        GameObject coinPrefab = CreateOrUpdateCoinPrefab();
        GameObject keyPrefab = CreateOrUpdateKeyPrefab();

        foreach (string prefabPath in SegmentPrefabPaths)
        {
            SetupSegmentPrefab(prefabPath, coinPrefab, keyPrefab);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Segment collectible setup completed.");
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Editor"))
        {
            AssetDatabase.CreateFolder("Assets", "Editor");
        }

        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Collectibles/Generated"))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs/Collectibles", "Generated");
        }
    }

    private static GameObject CreateOrUpdateCoinPrefab()
    {
        GameObject coinRoot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        coinRoot.name = "CoinPickup";
        coinRoot.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        coinRoot.transform.localScale = new Vector3(0.30156446f, 0.01206258f, 0.30156446f);

        MeshRenderer renderer = coinRoot.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(GoldCoinMaterialPath);

        CapsuleCollider collider = coinRoot.GetComponent<CapsuleCollider>();
        collider.isTrigger = true;
        collider.direction = 1;

        coinRoot.AddComponent<CollectableRotate>();
        CollectCoin collectCoin = coinRoot.AddComponent<CollectCoin>();
        AssignCoinPickupAudio(collectCoin);

        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(coinRoot, CoinPrefabPath);
        Object.DestroyImmediate(coinRoot);
        return savedPrefab;
    }

    private static void AssignCoinPickupAudio(CollectCoin collectCoin)
    {
        if (collectCoin == null)
        {
            return;
        }

        AudioClip coinSfx = AssetDatabase.LoadAssetAtPath<AudioClip>(CoinSfxPath);
        if (coinSfx == null)
        {
            return;
        }

        SerializedObject serializedCollectCoin = new SerializedObject(collectCoin);
        serializedCollectCoin.FindProperty("collectClip").objectReferenceValue = coinSfx;
        serializedCollectCoin.FindProperty("collectVolume").floatValue = 1f;
        serializedCollectCoin.ApplyModifiedPropertiesWithoutUndo();
    }

    private static GameObject CreateOrUpdateKeyPrefab()
    {
        GameObject keyRoot = GameObject.CreatePrimitive(PrimitiveType.Cube);
        keyRoot.name = "KeyPickup";
        keyRoot.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        keyRoot.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);

        MeshRenderer renderer = keyRoot.GetComponent<MeshRenderer>();
        renderer.enabled = false;

        BoxCollider collider = keyRoot.GetComponent<BoxCollider>();
        collider.isTrigger = true;

        CollectableRotate rotate = keyRoot.AddComponent<CollectableRotate>();
        SetRotationSpeed(rotate, 3f);
        keyRoot.AddComponent<CollectKey>();

        GameObject keyVisualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(KeyVisualPath);
        if (keyVisualPrefab != null)
        {
            GameObject visualInstance = PrefabUtility.InstantiatePrefab(keyVisualPrefab, keyRoot.transform) as GameObject;
            if (visualInstance != null)
            {
                visualInstance.name = "Key";
                visualInstance.transform.localPosition = Vector3.zero;
                visualInstance.transform.localRotation = Quaternion.identity;
                visualInstance.transform.localScale = Vector3.one * 50f;
            }
        }

        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(keyRoot, KeyPrefabPath);
        Object.DestroyImmediate(keyRoot);
        return savedPrefab;
    }

    private static void SetupSegmentPrefab(string prefabPath, GameObject coinPrefab, GameObject keyPrefab)
    {
        GameObject segmentRoot = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            RemoveManualCollectibles(segmentRoot);

            GameObject corridorTileHost = FindCorridorTileHost(segmentRoot);
            CorridorTile corridorTile = corridorTileHost != null ? corridorTileHost.GetComponent<CorridorTile>() : null;

            if (corridorTile == null && corridorTileHost != null)
            {
                corridorTile = corridorTileHost.AddComponent<CorridorTile>();
            }

            if (corridorTile == null)
            {
                Debug.LogWarning($"Skipping collectible setup for {prefabPath} because CorridorTile was not found.");
                PrefabUtility.SaveAsPrefabAsset(segmentRoot, prefabPath);
                return;
            }

            ConfigureCorridorTile(segmentRoot.transform, corridorTile);
            ClearLegacyCollectiblePrefabs(corridorTile);

            SegmentCollectibleSpawner spawner = corridorTile.GetComponent<SegmentCollectibleSpawner>();
            if (spawner == null)
            {
                spawner = corridorTile.gameObject.AddComponent<SegmentCollectibleSpawner>();
            }

            spawner.ApplyDefaultSetup(coinPrefab, keyPrefab);
            EditorUtility.SetDirty(spawner);
            EditorUtility.SetDirty(corridorTile);

            PrefabUtility.SaveAsPrefabAsset(segmentRoot, prefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(segmentRoot);
        }
    }

    private static GameObject FindCorridorTileHost(GameObject segmentRoot)
    {
        CorridorTile existingCorridorTile = segmentRoot.GetComponentInChildren<CorridorTile>(true);
        if (existingCorridorTile != null)
        {
            return existingCorridorTile.gameObject;
        }

        Transform namedTransform = FindDescendantByName(segmentRoot.transform, "CorridorTile");
        return namedTransform != null ? namedTransform.gameObject : null;
    }

    private static Transform FindDescendantByName(Transform root, string targetName)
    {
        if (root.name == targetName)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindDescendantByName(root.GetChild(i), targetName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static void ConfigureCorridorTile(Transform segmentRoot, CorridorTile corridorTile)
    {
        Transform corridorTransform = corridorTile.transform;
        Transform spawnPointsRoot = FindOrCreateChild(corridorTransform, "AutoSpawnPoints");
        spawnPointsRoot.localPosition = Vector3.zero;
        spawnPointsRoot.localRotation = Quaternion.identity;
        spawnPointsRoot.localScale = Vector3.one;

        Transform[] lanePoints = new Transform[LaneRootLocalPositions.Length];
        for (int i = 0; i < LaneRootLocalPositions.Length; i++)
        {
            Transform lanePoint = FindOrCreateChild(spawnPointsRoot, $"Lane_{i}");
            SetTransformAtRootLocalPosition(segmentRoot, corridorTransform, lanePoint, LaneRootLocalPositions[i]);
            lanePoints[i] = lanePoint;
        }

        Transform endNode = FindOrCreateChild(corridorTransform, "EndNode");
        SetTransformAtRootLocalPosition(segmentRoot, corridorTransform, endNode, EndNodeRootLocalPosition);

        corridorTile.endNode = endNode;
        corridorTile.lanePoints = lanePoints;
        corridorTile.fallbackTileLength = 30f;
        corridorTile.sideDecorPoints = new Transform[0];
        corridorTile.wallDecorPoints = new Transform[0];
        corridorTile.wallDecorPrefabs = new GameObject[0];
    }

    private static Transform FindOrCreateChild(Transform parent, string childName)
    {
        Transform existingChild = parent.Find(childName);
        if (existingChild != null)
        {
            return existingChild;
        }

        GameObject child = new GameObject(childName);
        child.transform.SetParent(parent, false);
        return child.transform;
    }

    private static void SetTransformAtRootLocalPosition(Transform segmentRoot, Transform corridorTransform, Transform marker, Vector3 rootLocalPosition)
    {
        Vector3 worldPosition = segmentRoot.TransformPoint(rootLocalPosition);
        marker.SetParent(corridorTransform, false);
        marker.position = worldPosition;
        marker.rotation = Quaternion.identity;
        marker.localScale = Vector3.one;
    }

    private static void RemoveManualCollectibles(GameObject segmentRoot)
    {
        HashSet<GameObject> objectsToRemove = new HashSet<GameObject>();

        foreach (CollectCoin coin in segmentRoot.GetComponentsInChildren<CollectCoin>(true))
        {
            if (coin != null)
            {
                objectsToRemove.Add(coin.gameObject);
            }
        }

        foreach (CollectKey key in segmentRoot.GetComponentsInChildren<CollectKey>(true))
        {
            if (key != null)
            {
                objectsToRemove.Add(key.gameObject);
            }
        }

        foreach (GameObject collectibleObject in objectsToRemove)
        {
            Object.DestroyImmediate(collectibleObject);
        }
    }

    private static void ClearLegacyCollectiblePrefabs(CorridorTile corridorTile)
    {
        SerializedObject serializedTile = new SerializedObject(corridorTile);
        SerializedProperty collectiblePrefabs = serializedTile.FindProperty("collectiblePrefabs");
        if (collectiblePrefabs != null)
        {
            collectiblePrefabs.arraySize = 0;
        }

        serializedTile.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetRotationSpeed(CollectableRotate rotate, float rotationSpeed)
    {
        SerializedObject serializedRotate = new SerializedObject(rotate);
        SerializedProperty speedProperty = serializedRotate.FindProperty("rotationSpeed");
        if (speedProperty != null)
        {
            speedProperty.floatValue = rotationSpeed;
            serializedRotate.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}

public static class SegmentPerformanceSetupTool
{
    private const int MaxRealtimePointLightsPerSegment = 3;

    private static readonly string[] SegmentPrefabPaths =
    {
        "Assets/Prefabs/Segment.prefab",
        "Assets/Prefabs/Segment (1).prefab",
        "Assets/Prefabs/Segment (2).prefab",
        "Assets/Prefabs/StartSegment.prefab"
    };

    private static readonly string[] HeavyObstaclePaths =
    {
        "Assets/Prefabs/Obstacles/Laundry basket.fbx",
        "Assets/Prefabs/Obstacles/Luggage Stack.fbx",
        "Assets/Prefabs/Obstacles/cleaning pot.fbx",
        "Assets/Prefabs/Obstacles/Luggage.fbx"
    };

    [MenuItem("Tools/Gameplay/Optimize Corridor Performance")]
    public static void RunBatchOptimization()
    {
        foreach (string prefabPath in SegmentPrefabPaths)
        {
            OptimizeSegmentLights(prefabPath);
        }

        foreach (string assetPath in HeavyObstaclePaths)
        {
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Corridor performance optimization completed.");
    }

    private static void OptimizeSegmentLights(string prefabPath)
    {
        GameObject segmentRoot = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            Light[] lights = segmentRoot.GetComponentsInChildren<Light>(true);
            List<Light> pointLights = new List<Light>();

            foreach (Light light in lights)
            {
                if (light == null)
                {
                    continue;
                }

                light.shadows = LightShadows.None;

                if (light.type == LightType.Point)
                {
                    pointLights.Add(light);
                }
            }

            pointLights.Sort((left, right) => ScoreLight(right).CompareTo(ScoreLight(left)));

            for (int i = 0; i < pointLights.Count; i++)
            {
                pointLights[i].enabled = i < MaxRealtimePointLightsPerSegment;
                EditorUtility.SetDirty(pointLights[i]);
            }

            PrefabUtility.SaveAsPrefabAsset(segmentRoot, prefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(segmentRoot);
        }
    }

    private static float ScoreLight(Light light)
    {
        return light.intensity * light.range;
    }
}
