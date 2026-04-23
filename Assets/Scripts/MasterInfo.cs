using UnityEngine;
using TMPro;

public class MasterInfo : MonoBehaviour
{
    [SerializeField] GameObject coinDisplay;
    [SerializeField] int internalDistance;
    [SerializeField] GameObject keyDisplay;
    [SerializeField] GameObject runDisplay;
    private TMP_Text _coinText;
    private TMP_Text _keyText;
    private TMP_Text _runText;
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
        }

        if (keyDisplay != null)
        {
            _keyText = keyDisplay.GetComponent<TMP_Text>();
        }

        if (runDisplay != null)
        {
            _runText = runDisplay.GetComponent<TMP_Text>();
        }
    }

    void Update()
    {
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
}
