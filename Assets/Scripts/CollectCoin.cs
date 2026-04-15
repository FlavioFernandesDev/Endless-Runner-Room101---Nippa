using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CollectCoin : MonoBehaviour
{
    [SerializeField] AudioSource coinFx;
    
    void OnTriggerEnter(Collider other)
    {
        coinFx.Play();
        MasterInfo.coinCount += 1;
        this.gameObject.SetActive(false);
    }
}
