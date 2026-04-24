using UnityEngine;
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
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        for (int i = 0; i < initialTilesCount; i++)
        {
            SpawnTile();
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

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
        
        CorridorTile corridorTile = tile.GetComponent<CorridorTile>();
        if (corridorTile != null)
        {
            nextSpawnPosition = new Vector3(nextSpawnPosition.x, nextSpawnPosition.y, corridorTile.GetNextSpawnZ());
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
