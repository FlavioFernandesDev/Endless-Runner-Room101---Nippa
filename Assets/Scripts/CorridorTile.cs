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

    private void Start()
    {
        SpawnObstacles();
        SpawnDecorations();
    }

    public float GetNextSpawnZ()
    {
        return endNode != null ? endNode.position.z : transform.position.z + fallbackTileLength;
    }

    private void SpawnObstacles()
    {
        if (lanePoints == null || lanePoints.Length == 0)
        {
            return;
        }

        int clearLane = Random.Range(0, lanePoints.Length);
        int maxBlocked = Mathf.Max(0, lanePoints.Length - 1);
        int desiredBlocked = RunManager.Instance.CurrentSpeed >= highSpeedThreshold ? highSpeedBlockedLanes : lowSpeedBlockedLanes;
        int blockedLanes = Mathf.Clamp(desiredBlocked, 0, maxBlocked);

        bool[] blocked = new bool[lanePoints.Length];
        int spawnedBlocked = 0;

        while (spawnedBlocked < blockedLanes)
        {
            int laneIndex = Random.Range(0, lanePoints.Length);
            if (laneIndex == clearLane || blocked[laneIndex])
            {
                continue;
            }

            blocked[laneIndex] = true;
            SpawnAtLane(laneIndex, obstaclePrefabs);
            spawnedBlocked += 1;
        }

        bool collectiblePlaced = false;
        for (int i = 0; i < lanePoints.Length; i++)
        {
            if (blocked[i])
            {
                continue;
            }

            bool isClearLane = i == clearLane;
            float collectibleChance = isClearLane ? 0.7f : 0.3f;
            if (!collectiblePlaced && Random.value <= collectibleChance)
            {
                collectiblePlaced = SpawnAtLane(i, collectiblePrefabs);
            }
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
                Instantiate(prefab, point.position, point.rotation, transform);
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
        Instantiate(prefab, lanePoint.position, lanePoint.rotation, transform);
        return true;
    }
}
