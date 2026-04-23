using System.Collections.Generic;
using UnityEngine;

public class SegmentCollectibleSpawner : MonoBehaviour
{
    private enum CoinPattern
    {
        SingleLane,
        ZigZag,
        DoubleLane,
        AllSafeLanes
    }

    [Header("Prefabs")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject keyPrefab;

    [Header("Spawn Layout")]
    [SerializeField] private float[] rowOffsets = { -9f, -6f, -3f, 0f, 3f, 6f, 9f };
    [SerializeField] private int minCoinRows = 4;
    [SerializeField] private int maxCoinRows = 6;
    [SerializeField] private Vector3 coinLocalOffset = new Vector3(0f, -0.5f, 0f);
    [SerializeField] private Vector3 keyLocalOffset = new Vector3(0f, -0.5f, 0f);
    [SerializeField] private bool limitToOneKeyPerSegment = true;

    [Header("Difficulty")]
    [SerializeField] private AnimationCurve keySpawnChanceByDistance = new AnimationCurve(
        new Keyframe(0f, 0.35f),
        new Keyframe(150f, 0.25f),
        new Keyframe(300f, 0.15f),
        new Keyframe(500f, 0.08f));

    [Header("Runtime")]
    [SerializeField] private string runtimeContainerName = "RuntimeCollectibles";

    private Transform _runtimeContainer;
    private int _lastZigZagLane = -1;

    public void ApplyDefaultSetup(GameObject newCoinPrefab, GameObject newKeyPrefab)
    {
        coinPrefab = newCoinPrefab;
        keyPrefab = newKeyPrefab;
        rowOffsets = new[] { -9f, -6f, -3f, 0f, 3f, 6f, 9f };
        minCoinRows = 4;
        maxCoinRows = 6;
        coinLocalOffset = new Vector3(0f, -0.5f, 0f);
        keyLocalOffset = new Vector3(0f, -0.5f, 0f);
        limitToOneKeyPerSegment = true;
        keySpawnChanceByDistance = CreateDefaultKeySpawnCurve();
    }

    private void Reset()
    {
        EnsureDefaults();
    }

    private void OnValidate()
    {
        EnsureDefaults();
        minCoinRows = Mathf.Max(0, minCoinRows);
        maxCoinRows = Mathf.Max(minCoinRows, maxCoinRows);
    }

    public void SpawnCollectibles(Transform[] lanePoints, bool[] blockedLanes, int clearLaneIndex, int runDistanceAtSpawn)
    {
        if (!isActiveAndEnabled || lanePoints == null || blockedLanes == null)
        {
            return;
        }

        if (lanePoints.Length == 0 || lanePoints.Length != blockedLanes.Length)
        {
            return;
        }

        List<int> safeLanes = GetSafeLanes(blockedLanes);
        if (safeLanes.Count == 0)
        {
            return;
        }

        List<int> candidateRows = GetCandidateRows();
        if (candidateRows.Count == 0)
        {
            return;
        }

        Transform container = GetOrCreateRuntimeContainer();
        ClearRuntimeContainer(container);

        _lastZigZagLane = -1;

        int desiredCoinRows = Mathf.Min(Random.Range(minCoinRows, maxCoinRows + 1), candidateRows.Count);
        List<int> shuffledRows = Shuffle(candidateRows);
        List<int> coinRows = shuffledRows.GetRange(0, desiredCoinRows);
        coinRows.Sort((left, right) => rowOffsets[left].CompareTo(rowOffsets[right]));

        HashSet<int> occupiedRows = new HashSet<int>();
        foreach (int rowIndex in coinRows)
        {
            SpawnCoinRow(lanePoints, safeLanes, clearLaneIndex, rowIndex, container);
            occupiedRows.Add(rowIndex);
        }

        if (keyPrefab == null)
        {
            return;
        }

        float keySpawnChance = Mathf.Clamp01(keySpawnChanceByDistance.Evaluate(runDistanceAtSpawn));
        if (Random.value > keySpawnChance)
        {
            return;
        }

        if (!limitToOneKeyPerSegment && safeLanes.Count > 0)
        {
            SpawnKey(lanePoints, safeLanes, clearLaneIndex, candidateRows, coinRows, container);
            return;
        }

        SpawnKey(lanePoints, safeLanes, clearLaneIndex, candidateRows, coinRows, container);
    }

    private void SpawnCoinRow(Transform[] lanePoints, List<int> safeLanes, int clearLaneIndex, int rowIndex, Transform container)
    {
        if (coinPrefab == null || safeLanes.Count == 0)
        {
            return;
        }

        List<int> targetLanes = SelectLanesForPattern(safeLanes, clearLaneIndex);
        float rowOffset = rowOffsets[rowIndex];

        foreach (int laneIndex in targetLanes)
        {
            SpawnPrefab(coinPrefab, lanePoints[laneIndex], rowOffset, coinLocalOffset, container);
        }
    }

    private void SpawnKey(
        Transform[] lanePoints,
        List<int> safeLanes,
        int clearLaneIndex,
        List<int> candidateRows,
        List<int> coinRows,
        Transform container)
    {
        List<int> availableRows = new List<int>();
        foreach (int rowIndex in candidateRows)
        {
            if (!coinRows.Contains(rowIndex))
            {
                availableRows.Add(rowIndex);
            }
        }

        if (availableRows.Count == 0)
        {
            return;
        }

        int rowToUse = GetMostReadableKeyRow(availableRows, coinRows);
        int laneToUse = ChoosePreferredLane(safeLanes, clearLaneIndex, -1);
        SpawnPrefab(keyPrefab, lanePoints[laneToUse], rowOffsets[rowToUse], keyLocalOffset, container);
    }

    private List<int> SelectLanesForPattern(List<int> safeLanes, int clearLaneIndex)
    {
        List<CoinPattern> patterns = GetAvailablePatterns(safeLanes.Count);
        CoinPattern selectedPattern = patterns[Random.Range(0, patterns.Count)];

        switch (selectedPattern)
        {
            case CoinPattern.ZigZag:
                return new List<int> { ChooseZigZagLane(safeLanes, clearLaneIndex) };
            case CoinPattern.DoubleLane:
                return GetDoubleLaneSelection(safeLanes, clearLaneIndex);
            case CoinPattern.AllSafeLanes:
                return OrderLanesByPriority(safeLanes, clearLaneIndex);
            default:
                _lastZigZagLane = -1;
                return new List<int> { ChoosePreferredLane(safeLanes, clearLaneIndex, -1) };
        }
    }

    private List<CoinPattern> GetAvailablePatterns(int safeLaneCount)
    {
        List<CoinPattern> patterns = new List<CoinPattern>
        {
            CoinPattern.SingleLane,
            CoinPattern.SingleLane
        };

        if (safeLaneCount >= 2)
        {
            patterns.Add(CoinPattern.ZigZag);
            patterns.Add(CoinPattern.DoubleLane);
        }

        if (safeLaneCount >= 3)
        {
            patterns.Add(CoinPattern.AllSafeLanes);
        }

        return patterns;
    }

    private int ChooseZigZagLane(List<int> safeLanes, int clearLaneIndex)
    {
        List<int> orderedSafeLanes = OrderLanesByPriority(safeLanes, clearLaneIndex);
        foreach (int laneIndex in orderedSafeLanes)
        {
            if (laneIndex != _lastZigZagLane)
            {
                _lastZigZagLane = laneIndex;
                return laneIndex;
            }
        }

        _lastZigZagLane = orderedSafeLanes[0];
        return orderedSafeLanes[0];
    }

    private List<int> GetDoubleLaneSelection(List<int> safeLanes, int clearLaneIndex)
    {
        List<int> orderedSafeLanes = OrderLanesByPriority(safeLanes, clearLaneIndex);
        List<int> result = new List<int> { orderedSafeLanes[0] };

        if (orderedSafeLanes.Count > 1)
        {
            result.Add(orderedSafeLanes[1]);
        }

        _lastZigZagLane = -1;
        return result;
    }

    private int ChoosePreferredLane(List<int> safeLanes, int clearLaneIndex, int excludedLane)
    {
        List<int> orderedSafeLanes = OrderLanesByPriority(safeLanes, clearLaneIndex);
        foreach (int laneIndex in orderedSafeLanes)
        {
            if (laneIndex != excludedLane)
            {
                return laneIndex;
            }
        }

        return orderedSafeLanes[0];
    }

    private List<int> OrderLanesByPriority(List<int> safeLanes, int clearLaneIndex)
    {
        List<int> orderedSafeLanes = new List<int>(safeLanes);
        orderedSafeLanes.Sort((left, right) =>
        {
            bool leftIsClear = left == clearLaneIndex;
            bool rightIsClear = right == clearLaneIndex;
            if (leftIsClear != rightIsClear)
            {
                return leftIsClear ? -1 : 1;
            }

            int centerDistanceCompare = Mathf.Abs(left - 1).CompareTo(Mathf.Abs(right - 1));
            if (centerDistanceCompare != 0)
            {
                return centerDistanceCompare;
            }

            return left.CompareTo(right);
        });

        return orderedSafeLanes;
    }

    private int GetMostReadableKeyRow(List<int> availableRows, List<int> coinRows)
    {
        if (coinRows.Count == 0)
        {
            return availableRows[Random.Range(0, availableRows.Count)];
        }

        int bestRow = availableRows[0];
        float bestSpacing = float.MinValue;

        foreach (int availableRow in availableRows)
        {
            float closestCoinDistance = float.MaxValue;
            foreach (int coinRow in coinRows)
            {
                float spacing = Mathf.Abs(rowOffsets[availableRow] - rowOffsets[coinRow]);
                closestCoinDistance = Mathf.Min(closestCoinDistance, spacing);
            }

            if (closestCoinDistance > bestSpacing)
            {
                bestSpacing = closestCoinDistance;
                bestRow = availableRow;
            }
        }

        return bestRow;
    }

    private void SpawnPrefab(GameObject prefab, Transform lanePoint, float rowOffset, Vector3 localOffset, Transform container)
    {
        if (prefab == null || lanePoint == null)
        {
            return;
        }

        Vector3 spawnPosition = lanePoint.position + new Vector3(localOffset.x, localOffset.y, rowOffset + localOffset.z);
        Instantiate(prefab, spawnPosition, prefab.transform.rotation, container);
    }

    private Transform GetOrCreateRuntimeContainer()
    {
        if (_runtimeContainer != null)
        {
            return _runtimeContainer;
        }

        Transform existingContainer = transform.Find(runtimeContainerName);
        if (existingContainer != null)
        {
            _runtimeContainer = existingContainer;
            return _runtimeContainer;
        }

        GameObject container = new GameObject(runtimeContainerName);
        container.transform.SetParent(transform, false);
        _runtimeContainer = container.transform;
        return _runtimeContainer;
    }

    private void ClearRuntimeContainer(Transform container)
    {
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            GameObject child = container.GetChild(i).gameObject;
            if (Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }
    }

    private List<int> GetSafeLanes(bool[] blockedLanes)
    {
        List<int> safeLanes = new List<int>();
        for (int i = 0; i < blockedLanes.Length; i++)
        {
            if (!blockedLanes[i])
            {
                safeLanes.Add(i);
            }
        }

        return safeLanes;
    }

    private List<int> GetCandidateRows()
    {
        List<int> candidateRows = new List<int>();
        if (rowOffsets == null)
        {
            return candidateRows;
        }

        for (int i = 0; i < rowOffsets.Length; i++)
        {
            candidateRows.Add(i);
        }

        return candidateRows;
    }

    private List<int> Shuffle(List<int> source)
    {
        List<int> shuffled = new List<int>(source);
        for (int i = 0; i < shuffled.Count; i++)
        {
            int swapIndex = Random.Range(i, shuffled.Count);
            int currentValue = shuffled[i];
            shuffled[i] = shuffled[swapIndex];
            shuffled[swapIndex] = currentValue;
        }

        return shuffled;
    }

    private void EnsureDefaults()
    {
        if (rowOffsets == null || rowOffsets.Length == 0)
        {
            rowOffsets = new[] { -9f, -6f, -3f, 0f, 3f, 6f, 9f };
        }

        if (keySpawnChanceByDistance == null || keySpawnChanceByDistance.length == 0)
        {
            keySpawnChanceByDistance = CreateDefaultKeySpawnCurve();
        }
    }

    private static AnimationCurve CreateDefaultKeySpawnCurve()
    {
        return new AnimationCurve(
            new Keyframe(0f, 0.35f),
            new Keyframe(150f, 0.25f),
            new Keyframe(300f, 0.15f),
            new Keyframe(500f, 0.08f));
    }
}
