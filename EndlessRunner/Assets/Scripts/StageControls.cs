using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class StageControls : MonoBehaviour
{
    // This script is just for the button, to take us to the next stage 
    void Start()
    {

    }

    void Update()
    {

    }

    public void PressPlayDesert()
    {
        LevelSelection.NextLevelBuildIndex = 1; // Desert Run
        SceneManager.LoadScene(3);              // Information
    }

    public void PressPlayIce()
    {
        LevelSelection.NextLevelBuildIndex = 4; // Ice Run
        SceneManager.LoadScene(3);              // Information
    }
}

