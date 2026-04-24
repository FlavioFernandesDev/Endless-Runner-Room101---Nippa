using UnityEngine;

public class CorridorTile : MonoBehaviour
{
    [Header("Structure")]
    public Transform endNode;

    [Header("Spawn Points")]
    public Transform[] lanePoints; // Left, Center, Right (X: -3, 0, 3)
    public Transform[] sideDecorPoints;
    public Transform[] wallDecorPoints;

    [Header("Prefabs for Randomization")]
    public GameObject[] obstaclePrefabs;
    public GameObject[] collectiblePrefabs;
    public GameObject[] wallDecorPrefabs;
    public int lowSpeedBlockedLanes = 1;
    public int highSpeedBlockedLanes = 2;
    public float highSpeedThreshold = 8f;
    public float fallbackTileLength = 30f;

    private SegmentCollectibleSpawner _collectibleSpawner;
    private Transform _runtimeSpawnContainer;
    private const string RuntimeSpawnContainerName = "RuntimeTileSpawns";

    private void Awake()
    {
        _collectibleSpawner = GetComponent<SegmentCollectibleSpawner>();
    }

    private void Start()
    {
        RegenerateRuntimeContent();
    }

    public void RegenerateRuntimeContent()
    {
        ClearRuntimeSpawnContainer();
        SpawnObstaclesAndCollectibles();
        SpawnDecorations();
        HauntedLevelStyler.ApplyTo(gameObject);
    }

    public float GetNextSpawnZ()
    {
        return endNode != null ? endNode.position.z : transform.position.z + fallbackTileLength;
    }

    private void SpawnObstaclesAndCollectibles()
    {
        if (lanePoints == null || lanePoints.Length == 0)
        {
            return;
        }

        int clearLane = Random.Range(0, lanePoints.Length);
        int maxBlocked = Mathf.Max(0, lanePoints.Length - 1);
        int desiredBlocked = RunManager.Instance.CurrentSpeed >= highSpeedThreshold ? highSpeedBlockedLanes : lowSpeedBlockedLanes;
        int blockedLanes = Mathf.Clamp(desiredBlocked, 0, maxBlocked);
        bool canSpawnObstacles = obstaclePrefabs != null && obstaclePrefabs.Length > 0;

        if (!canSpawnObstacles)
        {
            blockedLanes = 0;
        }

        bool[] blocked = new bool[lanePoints.Length];
        int spawnedBlocked = 0;

        while (spawnedBlocked < blockedLanes)
        {
            int laneIndex = Random.Range(0, lanePoints.Length);
            if (laneIndex == clearLane || blocked[laneIndex])
            {
                continue;
            }

            if (!SpawnAtLane(laneIndex, obstaclePrefabs))
            {
                break;
            }

            blocked[laneIndex] = true;
            spawnedBlocked += 1;
        }

        if (_collectibleSpawner != null)
        {
            _collectibleSpawner.SpawnCollectibles(lanePoints, blocked, clearLane, RunManager.Instance.CurrentDistance);
        }
    }

    private void SpawnDecorations()
    {
        if (wallDecorPoints == null || wallDecorPrefabs == null || wallDecorPrefabs.Length == 0)
        {
            return;
        }

        foreach (Transform point in wallDecorPoints)
        {
            if (point != null && Random.value > 0.6f)
            {
                GameObject prefab = wallDecorPrefabs[Random.Range(0, wallDecorPrefabs.Length)];
                Instantiate(prefab, point.position, point.rotation, GetOrCreateRuntimeSpawnContainer());
            }
        }
    }

    private bool SpawnAtLane(int laneIndex, GameObject[] prefabs)
    {
        if (prefabs == null || prefabs.Length == 0 || lanePoints == null || laneIndex < 0 || laneIndex >= lanePoints.Length)
        {
            return false;
        }

        Transform lanePoint = lanePoints[laneIndex];
        if (lanePoint == null)
        {
            return false;
        }

        GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
        Instantiate(prefab, lanePoint.position, lanePoint.rotation, GetOrCreateRuntimeSpawnContainer());
        return true;
    }

    private Transform GetOrCreateRuntimeSpawnContainer()
    {
        if (_runtimeSpawnContainer != null)
        {
            return _runtimeSpawnContainer;
        }

        Transform existingContainer = transform.Find(RuntimeSpawnContainerName);
        if (existingContainer != null)
        {
            _runtimeSpawnContainer = existingContainer;
            return _runtimeSpawnContainer;
        }

        GameObject container = new GameObject(RuntimeSpawnContainerName);
        container.transform.SetParent(transform, false);
        _runtimeSpawnContainer = container.transform;
        return _runtimeSpawnContainer;
    }

    private void ClearRuntimeSpawnContainer()
    {
        Transform container = GetOrCreateRuntimeSpawnContainer();
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            GameObject child = container.GetChild(i).gameObject;
            if (Application.isPlaying)
            {
                child.SetActive(false);
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }
    }
}
