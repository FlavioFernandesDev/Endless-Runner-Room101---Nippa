using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MasterInfo : MonoBehaviour
{
    public static int coinCount = 0;
    [SerializeField] GameObject coinDisplay;
    public static int keyCount=0;
    public static int distanceRun;
    [SerializeField] int internalDistance;
    [SerializeField] GameObject keyDisplay;
    [SerializeField] GameObject runDisplay;

    void Start ()
    {
       coinCount = 0;
       keyCount = 0;
       distanceRun = 0; 
    }

    void Update()
    {
        internalDistance = distanceRun;
        coinDisplay.GetComponent<TMPro.TMP_Text>().text = "" + coinCount;
        keyDisplay.GetComponent<TMPro.TMP_Text>().text = "" + keyCount;
        runDisplay.GetComponent<TMPro.TMP_Text>().text = "" + distanceRun;
    }
}
