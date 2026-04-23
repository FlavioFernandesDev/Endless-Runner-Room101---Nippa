using TMPro;
using UnityEngine;

public class SaveLoad : MonoBehaviour
{
    public static int loadedCoins => RunManager.Instance.TotalCoins;
    public static int loadedKeys => RunManager.Instance.CurrentKeys;
    public static int loadedDistance => RunManager.Instance.TotalDistance;

    [SerializeField] TMP_Text coinDisplay;
    [SerializeField] TMP_Text keyDisplay;
    [SerializeField] TMP_Text runDisplay;
    [SerializeField] int internalCoin;
    [SerializeField] int internalKey;
    [SerializeField] int internalDistance;

    private void Awake()
    {
        RefreshSnapshot();
    }

    private void OnEnable()
    {
        RefreshSnapshot();
    }

    [ContextMenu("Refresh Snapshot")]
    public void RefreshSnapshot()
    {
        if (RunManager.Instance == null)
        {
            return;
        }

        internalCoin = RunManager.Instance.TotalCoins;
        internalKey = RunManager.Instance.CurrentKeys;
        internalDistance = RunManager.Instance.TotalDistance;

        if (coinDisplay != null)
        {
            coinDisplay.text = internalCoin.ToString();
        }

        if (keyDisplay != null)
        {
            keyDisplay.text = internalKey.ToString();
        }

        if (runDisplay != null)
        {
            runDisplay.text = internalDistance.ToString();
        }
    }
}
