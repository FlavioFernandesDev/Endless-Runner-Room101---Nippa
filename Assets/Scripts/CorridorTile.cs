using UnityEngine;
using System.Collections.Generic;

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

    private void Start()
    {
        SpawnObstacles();
        SpawnDecorations();
    }

    private void SpawnObstacles()
    {
        // Simple logic: pick 1 or 2 lanes for obstacles, keep at least 1 clear.
        int clearLane = Random.Range(0, lanePoints.Length);
        
        for (int i = 0; i < lanePoints.Length; i++)
        {
            if (i == clearLane) continue;

            // 50% chance of spawning an obstacle in a non-clear lane
            if (Random.value > 0.5f && obstaclePrefabs.Length > 0)
            {
                GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
                Instantiate(prefab, lanePoints[i].position, Quaternion.identity, transform);
            }
            // 20% chance of spawning a collectible if no obstacle
            else if (Random.value > 0.8f && collectiblePrefabs.Length > 0)
            {
                GameObject prefab = collectiblePrefabs[Random.Range(0, collectiblePrefabs.Length)];
                Instantiate(prefab, lanePoints[i].position, Quaternion.identity, transform);
            }
        }
    }

    private void SpawnDecorations()
    {
        foreach (Transform point in wallDecorPoints)
        {
            if (Random.value > 0.6f && wallDecorPrefabs.Length > 0)
            {
                GameObject prefab = wallDecorPrefabs[Random.Range(0, wallDecorPrefabs.Length)];
                Instantiate(prefab, point.position, point.rotation, transform);
            }
        }
    }
}
