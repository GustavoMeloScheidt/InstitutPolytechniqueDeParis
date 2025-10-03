using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public void Restart()
    {
        // Goes back to the last phase played
        SceneManager.LoadScene(LevelSelection.NextLevelBuildIndex);
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(0); //Main menu
    }
}
