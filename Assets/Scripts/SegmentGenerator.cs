using UnityEngine;
using System.Collections.Generic;

public class SegmentGenerator : MonoBehaviour
{
    public GameObject[] segment;
    public Transform player;

    [SerializeField] float zPos = 30f;
    [SerializeField] int segmentNum;
    [SerializeField] int initialSegmentCount = 4;
    [SerializeField] int maxActiveSegments = 4;
    [SerializeField] float spawnTriggerDistance = 60f;
    [SerializeField] float fallbackSegmentLength = 30f;

    private readonly List<GameObject> activeSegments = new List<GameObject>();

    void Start()
    {
        TryResolvePlayer();

        for (int i = 0; i < initialSegmentCount; i++)
        {
            SpawnSegment();
        }
    }

    void Update()
    {
        if (player == null)
        {
            TryResolvePlayer();
        }

        if (player != null && player.position.z + spawnTriggerDistance > zPos)
        {
            SpawnSegment();
        }
    }

    void SpawnSegment()
    {
        if (segment == null || segment.Length == 0)
        {
            return;
        }

        segmentNum = Random.Range(0, segment.Length);
        GameObject newSegment = Instantiate(segment[segmentNum], new Vector3(0, 0, zPos), Quaternion.identity);
        ConfigureRuntimeReferences(newSegment);
        activeSegments.Add(newSegment);

        CorridorTile corridorTile = newSegment.GetComponentInChildren<CorridorTile>();
        if (corridorTile != null)
        {
            zPos = corridorTile.GetNextSpawnZ();
        }
        else
        {
            zPos += fallbackSegmentLength;
        }

        if (activeSegments.Count > maxActiveSegments)
        {
            Destroy(activeSegments[0]);
            activeSegments.RemoveAt(0);
        }
    }

    private void TryResolvePlayer()
    {
        if (player != null)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private void ConfigureRuntimeReferences(GameObject segmentRoot)
    {
        if (segmentRoot == null || player == null)
        {
            return;
        }

        RandomDoor[] doors = segmentRoot.GetComponentsInChildren<RandomDoor>(true);
        foreach (RandomDoor door in doors)
        {
            if (door != null)
            {
                door.SetPlayer(player);
            }
        }
    }
}
