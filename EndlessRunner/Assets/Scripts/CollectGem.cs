using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectGem : MonoBehaviour
{
    // Used to collect gems 
    [SerializeField] AudioSource gemFX;

    void OnTriggerEnter(Collider other)
    {
        gemFX.Play();
        MasterLevelInfo.gemCount += 1;
        this.gameObject.SetActive(false);
    }

}
