using UnityEngine;
using System.Collections.Generic;

public class SegmentGenerator : MonoBehaviour
{
    private sealed class ActiveSegment
    {
        public GameObject Instance;
        public GameObject Prefab;
    }

    public GameObject[] segment;
    public Transform player;

    [SerializeField] float zPos = 30f;
    [SerializeField] int segmentNum;
    [SerializeField] int initialSegmentCount = 4;
    [SerializeField] int maxActiveSegments = 4;
    [SerializeField] int prewarmSegmentCount = 5;
    [SerializeField] int spawnBatchLimitPerFrame = 1;
    [SerializeField] bool useSegmentPooling = true;
    [SerializeField] float spawnTriggerDistance = 90f;
    [SerializeField] float fallbackSegmentLength = 30f;

    private readonly List<ActiveSegment> activeSegments = new List<ActiveSegment>();
    private readonly Dictionary<GameObject, Queue<GameObject>> segmentPools = new Dictionary<GameObject, Queue<GameObject>>();

    private int ActiveSegmentLimit => Mathf.Max(maxActiveSegments, Mathf.Max(initialSegmentCount, prewarmSegmentCount));
    private float EffectiveSpawnTriggerDistance => Mathf.Max(spawnTriggerDistance, fallbackSegmentLength * 3f);

    void Start()
    {
        TryResolvePlayer();

        int segmentsToPrewarm = Mathf.Max(initialSegmentCount, prewarmSegmentCount);
        for (int i = 0; i < segmentsToPrewarm; i++)
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

        int spawnedThisFrame = 0;
        int spawnLimit = Mathf.Max(1, spawnBatchLimitPerFrame);
        while (player != null
            && player.position.z + EffectiveSpawnTriggerDistance > zPos
            && spawnedThisFrame < spawnLimit)
        {
            SpawnSegment();
            spawnedThisFrame += 1;
        }
    }

    void SpawnSegment()
    {
        if (segment == null || segment.Length == 0)
        {
            return;
        }

        segmentNum = Random.Range(0, segment.Length);
        GameObject prefab = segment[segmentNum];
        GameObject newSegment = GetSegmentInstance(prefab, new Vector3(0, 0, zPos), out bool reusedFromPool);
        ConfigureRuntimeReferences(newSegment, reusedFromPool);
        HauntedLevelStyler.ApplyTo(newSegment);
        activeSegments.Add(new ActiveSegment
        {
            Instance = newSegment,
            Prefab = prefab
        });

        CorridorTile corridorTile = newSegment.GetComponentInChildren<CorridorTile>();
        if (corridorTile != null)
        {
            zPos = corridorTile.GetNextSpawnZ();
        }
        else
        {
            zPos += fallbackSegmentLength;
        }

        if (activeSegments.Count > ActiveSegmentLimit)
        {
            ReleaseOldestSegment();
        }
    }

    private GameObject GetSegmentInstance(GameObject prefab, Vector3 position, out bool reusedFromPool)
    {
        reusedFromPool = false;
        if (useSegmentPooling && segmentPools.TryGetValue(prefab, out Queue<GameObject> pool) && pool.Count > 0)
        {
            GameObject pooledSegment = pool.Dequeue();
            pooledSegment.transform.SetPositionAndRotation(position, Quaternion.identity);
            pooledSegment.SetActive(true);
            reusedFromPool = true;
            return pooledSegment;
        }

        return Instantiate(prefab, position, Quaternion.identity, transform);
    }

    private void ReleaseOldestSegment()
    {
        ActiveSegment oldestSegment = activeSegments[0];
        activeSegments.RemoveAt(0);

        if (oldestSegment.Instance == null)
        {
            return;
        }

        if (useSegmentPooling && oldestSegment.Prefab != null)
        {
            if (!segmentPools.TryGetValue(oldestSegment.Prefab, out Queue<GameObject> pool))
            {
                pool = new Queue<GameObject>();
                segmentPools.Add(oldestSegment.Prefab, pool);
            }

            oldestSegment.Instance.SetActive(false);
            pool.Enqueue(oldestSegment.Instance);
            return;
        }

        Destroy(oldestSegment.Instance);
    }

    private void OnValidate()
    {
        initialSegmentCount = Mathf.Max(1, initialSegmentCount);
        maxActiveSegments = Mathf.Max(1, maxActiveSegments);
        prewarmSegmentCount = Mathf.Max(1, prewarmSegmentCount);
        spawnBatchLimitPerFrame = Mathf.Max(1, spawnBatchLimitPerFrame);
        spawnTriggerDistance = Mathf.Max(0f, spawnTriggerDistance);
        fallbackSegmentLength = Mathf.Max(1f, fallbackSegmentLength);
    }

    private void OnDestroy()
    {
        foreach (Queue<GameObject> pool in segmentPools.Values)
        {
            while (pool.Count > 0)
            {
                GameObject pooledSegment = pool.Dequeue();
                if (pooledSegment != null)
                {
                    Destroy(pooledSegment);
                }
            }
        }

        segmentPools.Clear();
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

    private void ConfigureRuntimeReferences(GameObject segmentRoot, bool regenerateRuntimeContent)
    {
        if (segmentRoot == null || player == null)
        {
            return;
        }

        RandomDoor[] doors = segmentRoot.GetComponentsInChildren<RandomDoor>(true);
        foreach (RandomDoor door in doors)
        {
            if (door == null)
            {
                continue;
            }

            door.SetPlayer(player);
            if (regenerateRuntimeContent)
            {
                door.ResetRuntimeState();
            }
        }

        if (!regenerateRuntimeContent)
        {
            return;
        }

        CorridorTile corridorTile = segmentRoot.GetComponentInChildren<CorridorTile>();
        if (corridorTile != null)
        {
            corridorTile.RegenerateRuntimeContent();
        }
    }
}
