using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class RunManager
{
    public const string MainMenuSceneName = "MainMenu";
    public const string StageSelectSceneName = "StageSelect";
    public const string InformationSceneName = "Information";
    public const string GameplaySceneName = "HotelCorridor1";

    private const string CoinSaveKey = "COINSAVE";
    private const string KeySaveKey = "KEYSAVE";
    private const string DistanceSaveKey = "DISTANCESAVE";

    private static RunManager _instance;
    private bool _bootstrapped;
    private bool _totalsLoaded;
    private bool _runCommitted;
    private float _distanceAccumulator;

    public static RunManager Instance => _instance ??= new RunManager();

    public int CurrentCoins { get; private set; }
    public int CurrentKeys { get; private set; }
    public int CurrentDistance => Mathf.FloorToInt(_distanceAccumulator);
    public int TotalCoins { get; private set; }
    public int TotalKeys { get; private set; }
    public int TotalDistance { get; private set; }
    public float CurrentSpeed { get; private set; }
    public bool IsPaused { get; private set; }
    public bool IsGameOver { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        Instance.EnsureBootstrapped();
    }

    private void EnsureBootstrapped()
    {
        if (_bootstrapped)
        {
            return;
        }

        _bootstrapped = true;
        LoadTotals();
        SceneManager.sceneLoaded += HandleSceneLoaded;
        HandleSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
        IsPaused = false;
        CurrentSpeed = 0f;

        if (scene.name == GameplaySceneName)
        {
            ResetRunState();
            return;
        }

        ResetTransientState();
    }

    private void ResetRunState()
    {
        CurrentCoins = 0;
        CurrentKeys = 0;
        _distanceAccumulator = 0f;
        CurrentSpeed = 0f;
        IsPaused = false;
        IsGameOver = false;
        _runCommitted = false;
    }

    private void ResetTransientState()
    {
        CurrentCoins = 0;
        CurrentKeys = 0;
        _distanceAccumulator = 0f;
        CurrentSpeed = 0f;
        IsPaused = false;
        IsGameOver = false;
        _runCommitted = false;
    }

    private void LoadTotals()
    {
        if (_totalsLoaded)
        {
            return;
        }

        _totalsLoaded = true;
        TotalCoins = PlayerPrefs.GetInt(CoinSaveKey, 0);
        TotalKeys = PlayerPrefs.GetInt(KeySaveKey, 0);
        TotalDistance = PlayerPrefs.GetInt(DistanceSaveKey, 0);
    }

    private bool IsGameplaySceneActive()
    {
        return SceneManager.GetActiveScene().name == GameplaySceneName;
    }

    public void SetForwardSpeed(float speed)
    {
        if (!IsGameplaySceneActive())
        {
            return;
        }

        CurrentSpeed = Mathf.Max(0f, speed);
    }

    public void AddCoin()
    {
        if (!IsGameplaySceneActive() || IsGameOver)
        {
            return;
        }

        CurrentCoins += 1;
    }

    public void AddKey()
    {
        if (!IsGameplaySceneActive() || IsGameOver)
        {
            return;
        }

        CurrentKeys += 1;
    }

    public bool TryConsumeKey()
    {
        if (!IsGameplaySceneActive() || IsGameOver || CurrentKeys <= 0)
        {
            return false;
        }

        CurrentKeys -= 1;
        return true;
    }

    public void AddDistance(float distance)
    {
        if (!IsGameplaySceneActive() || IsGameOver || distance <= 0f)
        {
            return;
        }

        _distanceAccumulator += distance;
    }

    public void PauseRun()
    {
        if (!IsGameplaySceneActive() || IsGameOver || IsPaused)
        {
            return;
        }

        Time.timeScale = 0f;
        IsPaused = true;
    }

    public void ResumeRun()
    {
        if (!IsPaused)
        {
            return;
        }

        Time.timeScale = 1f;
        IsPaused = false;
    }

    public void EndRun()
    {
        if (!IsGameplaySceneActive() || IsGameOver)
        {
            return;
        }

        IsGameOver = true;
        CommitRunTotals();
    }

    public void ExitToStageSelect()
    {
        ResumeRun();
        SceneManager.LoadScene(StageSelectSceneName);
    }

    private void CommitRunTotals()
    {
        if (_runCommitted)
        {
            return;
        }

        _runCommitted = true;
        TotalCoins += CurrentCoins;
        TotalKeys += CurrentKeys;
        TotalDistance += CurrentDistance;

        PlayerPrefs.SetInt(CoinSaveKey, TotalCoins);
        PlayerPrefs.SetInt(KeySaveKey, TotalKeys);
        PlayerPrefs.SetInt(DistanceSaveKey, TotalDistance);
        PlayerPrefs.Save();
    }
}
