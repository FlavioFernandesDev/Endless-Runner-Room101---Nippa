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
    private readonly List<int> _safeLanes = new List<int>();
    private readonly List<int> _candidateRows = new List<int>();
    private readonly List<int> _coinRows = new List<int>();
    private readonly List<int> _availableRows = new List<int>();
    private readonly List<int> _orderedSafeLanes = new List<int>();
    private readonly List<int> _targetLanes = new List<int>();
    private readonly List<CoinPattern> _availablePatterns = new List<CoinPattern>();

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

        PopulateSafeLanes(blockedLanes);
        if (_safeLanes.Count == 0)
        {
            return;
        }

        PopulateCandidateRows();
        if (_candidateRows.Count == 0)
        {
            return;
        }

        Transform container = GetOrCreateRuntimeContainer();
        ClearRuntimeContainer(container);

        _lastZigZagLane = -1;

        int desiredCoinRows = Mathf.Min(Random.Range(minCoinRows, maxCoinRows + 1), _candidateRows.Count);
        ShuffleInPlace(_candidateRows);
        _coinRows.Clear();
        for (int i = 0; i < desiredCoinRows; i++)
        {
            _coinRows.Add(_candidateRows[i]);
        }

        _coinRows.Sort((left, right) => rowOffsets[left].CompareTo(rowOffsets[right]));

        foreach (int rowIndex in _coinRows)
        {
            SpawnCoinRow(lanePoints, _safeLanes, clearLaneIndex, rowIndex, container);
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

        if (limitToOneKeyPerSegment)
        {
            SpawnKey(lanePoints, _safeLanes, clearLaneIndex, _candidateRows, _coinRows, container);
            return;
        }

        SpawnKey(lanePoints, _safeLanes, clearLaneIndex, _candidateRows, _coinRows, container);
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
        _availableRows.Clear();
        foreach (int rowIndex in candidateRows)
        {
            if (!coinRows.Contains(rowIndex))
            {
                _availableRows.Add(rowIndex);
            }
        }

        if (_availableRows.Count == 0)
        {
            return;
        }

        int rowToUse = GetMostReadableKeyRow(_availableRows, coinRows);
        int laneToUse = ChoosePreferredLane(safeLanes, clearLaneIndex, -1);
        SpawnPrefab(keyPrefab, lanePoints[laneToUse], rowOffsets[rowToUse], keyLocalOffset, container);
    }

    private List<int> SelectLanesForPattern(List<int> safeLanes, int clearLaneIndex)
    {
        List<CoinPattern> patterns = GetAvailablePatterns(safeLanes.Count);
        CoinPattern selectedPattern = patterns[Random.Range(0, patterns.Count)];
        _targetLanes.Clear();

        switch (selectedPattern)
        {
            case CoinPattern.ZigZag:
                _targetLanes.Add(ChooseZigZagLane(safeLanes, clearLaneIndex));
                return _targetLanes;
            case CoinPattern.DoubleLane:
                return GetDoubleLaneSelection(safeLanes, clearLaneIndex);
            case CoinPattern.AllSafeLanes:
                return OrderLanesByPriority(safeLanes, clearLaneIndex);
            default:
                _lastZigZagLane = -1;
                _targetLanes.Add(ChoosePreferredLane(safeLanes, clearLaneIndex, -1));
                return _targetLanes;
        }
    }

    private List<CoinPattern> GetAvailablePatterns(int safeLaneCount)
    {
        _availablePatterns.Clear();
        _availablePatterns.Add(CoinPattern.SingleLane);
        _availablePatterns.Add(CoinPattern.SingleLane);

        if (safeLaneCount >= 2)
        {
            _availablePatterns.Add(CoinPattern.ZigZag);
            _availablePatterns.Add(CoinPattern.DoubleLane);
        }

        if (safeLaneCount >= 3)
        {
            _availablePatterns.Add(CoinPattern.AllSafeLanes);
        }

        return _availablePatterns;
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
        _targetLanes.Clear();
        _targetLanes.Add(orderedSafeLanes[0]);

        if (orderedSafeLanes.Count > 1)
        {
            _targetLanes.Add(orderedSafeLanes[1]);
        }

        _lastZigZagLane = -1;
        return _targetLanes;
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
        _orderedSafeLanes.Clear();
        _orderedSafeLanes.AddRange(safeLanes);
        _orderedSafeLanes.Sort((left, right) =>
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

        return _orderedSafeLanes;
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
                child.SetActive(false);
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }
    }

    private void PopulateSafeLanes(bool[] blockedLanes)
    {
        _safeLanes.Clear();
        for (int i = 0; i < blockedLanes.Length; i++)
        {
            if (!blockedLanes[i])
            {
                _safeLanes.Add(i);
            }
        }
    }

    private void PopulateCandidateRows()
    {
        _candidateRows.Clear();
        if (rowOffsets == null)
        {
            return;
        }

        for (int i = 0; i < rowOffsets.Length; i++)
        {
            _candidateRows.Add(i);
        }
    }

    private void ShuffleInPlace(List<int> source)
    {
        for (int i = 0; i < source.Count; i++)
        {
            int swapIndex = Random.Range(i, source.Count);
            int currentValue = source[i];
            source[i] = source[swapIndex];
            source[swapIndex] = currentValue;
        }
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
