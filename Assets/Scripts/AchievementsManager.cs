using System.Collections.Generic;
using UnityEngine;

public enum AchievementProgressType
{
    BestRunDistance,
    TotalCoins,
    TotalKeys
}

public readonly struct AchievementDefinition
{
    public AchievementDefinition(string id, string titleKey, string descriptionKey, string category, int targetValue, AchievementProgressType progressType)
    {
        Id = id;
        TitleKey = titleKey;
        DescriptionKey = descriptionKey;
        Category = category;
        TargetValue = targetValue;
        ProgressType = progressType;
    }

    public string Id { get; }
    public string TitleKey { get; }
    public string DescriptionKey { get; }
    public string Category { get; }
    public int TargetValue { get; }
    public AchievementProgressType ProgressType { get; }
}

public static class AchievementsManager
{
    private const string BestRunDistanceKey = "ACHIEVEMENTS_BEST_RUN_DISTANCE";
    private const string UnlockPrefix = "ACHIEVEMENT_UNLOCKED_";

    private static readonly AchievementDefinition[] Definitions =
    {
        new("distance_100", "ach.distance.100.title", "ach.distance.100.desc", "Distance", 100, AchievementProgressType.BestRunDistance),
        new("distance_250", "ach.distance.250.title", "ach.distance.250.desc", "Distance", 250, AchievementProgressType.BestRunDistance),
        new("distance_500", "ach.distance.500.title", "ach.distance.500.desc", "Distance", 500, AchievementProgressType.BestRunDistance),
        new("distance_1000", "ach.distance.1000.title", "ach.distance.1000.desc", "Distance", 1000, AchievementProgressType.BestRunDistance),
        new("coins_25", "ach.coins.25.title", "ach.coins.25.desc", "Coins", 25, AchievementProgressType.TotalCoins),
        new("coins_100", "ach.coins.100.title", "ach.coins.100.desc", "Coins", 100, AchievementProgressType.TotalCoins),
        new("coins_250", "ach.coins.250.title", "ach.coins.250.desc", "Coins", 250, AchievementProgressType.TotalCoins),
        new("keys_3", "ach.keys.3.title", "ach.keys.3.desc", "Keys", 3, AchievementProgressType.TotalKeys),
        new("keys_10", "ach.keys.10.title", "ach.keys.10.desc", "Keys", 10, AchievementProgressType.TotalKeys),
        new("keys_25", "ach.keys.25.title", "ach.keys.25.desc", "Keys", 25, AchievementProgressType.TotalKeys)
    };

    private static bool _loaded;
    private static int _bestRunDistance;
    private static readonly HashSet<string> UnlockedIds = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        Load();
    }

    public static void Load()
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        _bestRunDistance = PlayerPrefs.GetInt(BestRunDistanceKey, 0);
        UnlockedIds.Clear();

        foreach (AchievementDefinition definition in Definitions)
        {
            if (PlayerPrefs.GetInt(GetUnlockKey(definition.Id), 0) == 1)
            {
                UnlockedIds.Add(definition.Id);
            }
        }
    }

    public static void EvaluateAfterRun(int runDistance, int totalCoins, int totalKeys)
    {
        Load();

        if (runDistance > _bestRunDistance)
        {
            _bestRunDistance = runDistance;
            PlayerPrefs.SetInt(BestRunDistanceKey, _bestRunDistance);
        }

        foreach (AchievementDefinition definition in Definitions)
        {
            if (UnlockedIds.Contains(definition.Id))
            {
                continue;
            }

            if (GetProgressValue(definition.ProgressType, totalCoins, totalKeys) >= definition.TargetValue)
            {
                UnlockedIds.Add(definition.Id);
                PlayerPrefs.SetInt(GetUnlockKey(definition.Id), 1);
            }
        }

        PlayerPrefs.Save();
    }

    public static IReadOnlyList<AchievementDefinition> GetAllAchievements()
    {
        Load();
        return Definitions;
    }

    public static bool IsUnlocked(string id)
    {
        Load();
        return UnlockedIds.Contains(id);
    }

    public static int GetUnlockedCount()
    {
        Load();
        return UnlockedIds.Count;
    }

    public static float GetProgressPercent(AchievementDefinition definition)
    {
        if (definition.TargetValue <= 0)
        {
            return 0f;
        }

        return Mathf.Clamp01(GetProgress(definition) / (float)definition.TargetValue);
    }

    public static string GetCategoryKey(AchievementProgressType progressType)
    {
        return progressType switch
        {
            AchievementProgressType.BestRunDistance => "ach.category.distance",
            AchievementProgressType.TotalCoins => "ach.category.coins",
            AchievementProgressType.TotalKeys => "ach.category.keys",
            _ => "ach.category.progress"
        };
    }

    public static string GetProgressFormatKey(AchievementProgressType progressType)
    {
        return progressType switch
        {
            AchievementProgressType.BestRunDistance => "ach.progress.distance",
            AchievementProgressType.TotalCoins => "ach.progress.coins",
            AchievementProgressType.TotalKeys => "ach.progress.keys",
            _ => "ach.progress.value"
        };
    }

    public static int GetBestRunDistance()
    {
        Load();
        return _bestRunDistance;
    }

    public static int GetProgress(AchievementDefinition definition)
    {
        Load();
        return Mathf.Min(definition.TargetValue, GetProgressValue(definition.ProgressType, RunManager.Instance.TotalCoins, RunManager.Instance.TotalKeys));
    }

    private static int GetProgressValue(AchievementProgressType progressType, int totalCoins, int totalKeys)
    {
        return progressType switch
        {
            AchievementProgressType.BestRunDistance => _bestRunDistance,
            AchievementProgressType.TotalCoins => totalCoins,
            AchievementProgressType.TotalKeys => totalKeys,
            _ => 0
        };
    }

    private static string GetUnlockKey(string id)
    {
        return UnlockPrefix + id;
    }
}
