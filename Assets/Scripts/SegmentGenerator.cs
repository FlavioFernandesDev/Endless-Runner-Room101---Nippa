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

    private List<GameObject> activeSegments = new List<GameObject>();

    void Start()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        for (int i = 0; i < initialSegmentCount; i++)
        {
            SpawnSegment();
        }
    }

    void Update()
    {
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
}
