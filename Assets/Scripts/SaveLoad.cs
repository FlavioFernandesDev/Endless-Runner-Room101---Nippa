using UnityEngine;

public class SaveLoad : MonoBehaviour
{
    public static int loadedCoins => RunManager.Instance.TotalCoins;
    public static int loadedKeys => RunManager.Instance.TotalKeys;
    public static int loadedDistance => RunManager.Instance.TotalDistance;

    [SerializeField] int internalCoin;
    [SerializeField] int internalKey;
    [SerializeField] int internalDistance;

    void Update()
    {
        internalCoin = RunManager.Instance.TotalCoins;
        internalKey = RunManager.Instance.TotalKeys;
        internalDistance = RunManager.Instance.TotalDistance;
    }
}
