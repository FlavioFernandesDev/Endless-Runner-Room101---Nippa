using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MasterInfo : MonoBehaviour
{
    [SerializeField] GameObject coinDisplay;
    [SerializeField] int internalDistance;
    [SerializeField] GameObject keyDisplay;
    [SerializeField] GameObject runDisplay;
    private TMP_Text _coinText;
    private TMP_Text _keyText;
    private TMP_Text _runText;
    private Color _defaultCoinTextColor = Color.black;
    private Color _defaultKeyTextColor = Color.black;
    private Color _defaultRunTextColor = Color.black;
    private bool? _lastHauntedTextState;
    private int _lastCoins = -1;
    private int _lastKeys = -1;
    private int _lastDistance = -1;

    public static int coinCount => RunManager.Instance.CurrentCoins;
    public static int keyCount => RunManager.Instance.CurrentKeys;
    public static int distanceRun => RunManager.Instance.CurrentDistance;

    void Awake()
    {
        if (coinDisplay != null)
        {
            _coinText = coinDisplay.GetComponent<TMP_Text>();
            if (_coinText != null)
            {
                _defaultCoinTextColor = _coinText.color;
            }
        }

        if (keyDisplay != null)
        {
            _keyText = keyDisplay.GetComponent<TMP_Text>();
            if (_keyText != null)
            {
                _defaultKeyTextColor = _keyText.color;
            }
        }

        if (runDisplay != null)
        {
            _runText = runDisplay.GetComponent<TMP_Text>();
            if (_runText != null)
            {
                _defaultRunTextColor = _runText.color;
            }
        }

        ApplySceneTextColorIfNeeded();
    }

    void Update()
    {
        ApplySceneTextColorIfNeeded();

        int coins = RunManager.Instance.CurrentCoins;
        int keys = RunManager.Instance.CurrentKeys;
        int distance = RunManager.Instance.CurrentDistance;

        internalDistance = distance;

        if (_coinText != null && _lastCoins != coins)
        {
            _coinText.text = coins.ToString();
            _lastCoins = coins;
        }

        if (_keyText != null && _lastKeys != keys)
        {
            _keyText.text = keys.ToString();
            _lastKeys = keys;
        }

        if (_runText != null && _lastDistance != distance)
        {
            _runText.text = distance.ToString();
            _lastDistance = distance;
        }
    }

    private void ApplySceneTextColorIfNeeded()
    {
        bool isHauntedGameplay = SceneManager.GetActiveScene().name == RunManager.HauntedGameplaySceneName;
        if (_lastHauntedTextState == isHauntedGameplay)
        {
            return;
        }

        _lastHauntedTextState = isHauntedGameplay;
        SetTextColor(_coinText, isHauntedGameplay ? Color.white : _defaultCoinTextColor);
        SetTextColor(_keyText, isHauntedGameplay ? Color.white : _defaultKeyTextColor);
        SetTextColor(_runText, isHauntedGameplay ? Color.white : _defaultRunTextColor);
    }

    private static void SetTextColor(TMP_Text text, Color color)
    {
        if (text != null)
        {
            text.color = color;
        }
    }
}
