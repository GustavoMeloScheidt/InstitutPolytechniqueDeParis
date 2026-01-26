using UnityEngine;

public class MoleTouchHit : MonoBehaviour {
    [Header("References")]
    [Tooltip("Arraste o script Mole principal aqui")]
    [SerializeField] private Mole moleScript;

    [Header("Configuration")]
    [Tooltip("A Tag que a mao do MRTK ou o martelo usa. Padrao costuma ser 'Player' ou 'Hand'")]
    [SerializeField] private string handTag = "Hammer";

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag(handTag)) {
            moleScript.TryHit();
        }
    }

/*     void OnCollisionEnter(Collision collision)
    {
        moleScript.TryHit();
    } */
}