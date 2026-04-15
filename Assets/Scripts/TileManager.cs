using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileManager : MonoBehaviour
{
    [Header("Generation Settings")]
    public GameObject tilePrefab;
    public int initialTilesCount = 10;
    public float tileLength = 10f;
    
    [Header("Player Reference")]
    public Transform playerTransform;
    public float destroyDistance = 15f; // Distance behind player to destroy tile

    private List<GameObject> activeTiles = new List<GameObject>();
    private Vector3 nextSpawnPosition = Vector3.zero;

    private void Start()
    {
        for (int i = 0; i < initialTilesCount; i++)
        {
            SpawnTile();
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        // Check if the last spawned tile is within range to spawn another
        // Actually, a simpler way: if the first tile is too far behind, destroy it and spawn a new one at the end.
        if (activeTiles.Count > 0 && playerTransform.position.z - destroyDistance > activeTiles[0].transform.position.z + tileLength)
        {
            DestroyOldestTile();
            SpawnTile();
        }
    }

    private void SpawnTile()
    {
        GameObject tile = Instantiate(tilePrefab, nextSpawnPosition, Quaternion.identity, transform);
        activeTiles.Add(tile);
        
        // Find the end node to get the next position
        CorridorTile corridorTile = tile.GetComponent<CorridorTile>();
        if (corridorTile != null && corridorTile.endNode != null)
        {
            nextSpawnPosition = corridorTile.endNode.position;
        }
        else
        {
            nextSpawnPosition.z += tileLength;
        }
    }

    private void DestroyOldestTile()
    {
        GameObject oldest = activeTiles[0];
        activeTiles.RemoveAt(0);
        Destroy(oldest);
    }
}
