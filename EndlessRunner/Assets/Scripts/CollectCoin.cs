using UnityEngine;

public class CollectCoin : MonoBehaviour
{
    // Script to trigger the event of the sound of the coin and make it disappear
    [SerializeField] AudioSource coinFX;

    void OnTriggerEnter(Collider other)
    {
        coinFX.Play();
        MasterLevelInfo.coinCount += 1;
        this.gameObject.SetActive(false);
    }
    
}
