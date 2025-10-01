using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadToStage : MonoBehaviour
{
    [SerializeField] GameObject fadeOut;
    // This script is designed to take us to the level (from the information scene)
    void Start()
    {
        //fazer um if pressplay 1(desert run), ir para coroutine loadlevel 1, if pressplay2, ir para coroutine loadlevel2(ice run)
        StartCoroutine(LoadLevel());

    }

    IEnumerator LoadLevel()
    {
        yield return new WaitForSeconds(3);
        fadeOut.SetActive(true);
        yield return new WaitForSeconds(2);
        //SceneManager.LoadScene(1); 
        // goes to the scene that was selected in the Stage Select
        SceneManager.LoadScene(LevelSelection.NextLevelBuildIndex);
    }
}
