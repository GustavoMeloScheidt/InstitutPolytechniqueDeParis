using UnityEngine;

public class MasterLevelInfo : MonoBehaviour
{
    // Infos for the level (such as coins amount)
    public static int coinCount = 0;
    public static int gemCount = 0;
    public static int distanceRun;

    [SerializeField] GameObject coinDisplay;
    [SerializeField] GameObject gemDisplay;
    [SerializeField] int internalDistance;



    void Update()
    {
        internalDistance = distanceRun;
        coinDisplay.GetComponent<TMPro.TMP_Text>().text = "COINS: " + coinCount;   
        gemDisplay.GetComponent<TMPro.TMP_Text>().text = "GEMS: " + gemCount; 
    }
}
