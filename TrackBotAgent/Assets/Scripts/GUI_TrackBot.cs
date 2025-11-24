using UnityEngine;

public class GUI_TrackBot : MonoBehaviour
{
    // Reference to the TrackBotAgent whose stats will be displayed
    [SerializeField] private TrackBot _trackBotAgent;

    // Default style for generic text (episode / step)
    private GUIStyle _defaultStyle = new GUIStyle();
    // Style used when the cumulative reward is positive
    private GUIStyle _positiveStyle = new GUIStyle();
    // Style used when the cumulative reward is negative
    private GUIStyle _negativeStyle = new GUIStyle();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Define GUI styles
        _defaultStyle.fontSize = 20;
        _defaultStyle.normal.textColor = Color.yellow;

        _positiveStyle.fontSize = 20;
        _positiveStyle.normal.textColor = Color.green;

        _negativeStyle.fontSize = 20;
        _negativeStyle.normal.textColor = Color.red;
    }

    // Called by Unity to draw and handle GUI events
    private void OnGUI()
    {
        // Build debug strings for episode/step and current reward
        string debugEpisode = "Episode: " + _trackBotAgent.CurrentEpisode + " - Step: " + _trackBotAgent.StepCount;
        string debugReward = "Reward: " + _trackBotAgent.CumulativeReward.ToString();

        // Select style based on reward value
        GUIStyle rewardStyle = _trackBotAgent.CumulativeReward < 0 ? _negativeStyle : _positiveStyle;

        // Display the debug text
        GUI.Label(new Rect(20, 20, 500, 30), debugEpisode, _defaultStyle);
        GUI.Label(new Rect(20, 60, 500, 30), debugReward, rewardStyle);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
