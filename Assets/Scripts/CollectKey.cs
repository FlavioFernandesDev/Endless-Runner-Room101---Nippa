using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CollectGem : MonoBehaviour
{
    [SerializeField] AudioSource coinFx;
    
    void OnTriggerEnter(Collider other)
    {
        coinFx.Play();
        MasterInfo.keyCount += 1;
        this.gameObject.SetActive(false);
    }
}
