using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class SaveLoad : MonoBehaviour
{
    public static int loadedCoins;
    public static int loadedKeys;
    public static int loadedDistance;

    public static bool saveData;

    [SerializeField] int internalCoin;
    [SerializeField] int internalKey;
    [SerializeField] int internalDistance;

    void Start()
    {
        loadedCoins = PlayerPrefs.GetInt("COINSAVE");
        loadedKeys = PlayerPrefs.GetInt("KEYSAVE");
        loadedDistance = PlayerPrefs.GetInt("DISTANCESAVE");
    }

    
    void Update()
    {
        internalCoin = loadedCoins + MasterInfo.coinCount;
        internalKey = loadedKeys + MasterInfo.keyCount;
        internalDistance = loadedDistance + MasterInfo.distanceRun;
        if (saveData == true)
        {
            saveData = false;
            PlayerPrefs.SetInt("COINSAVE", internalCoin);
            PlayerPrefs.SetInt("KEYSAVE", internalKey);
            PlayerPrefs.SetInt("DISTANCESAVE", internalDistance);
        }
    }
}
