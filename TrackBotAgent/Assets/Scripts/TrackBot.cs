using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections;

public class TrackBot : Agent
{
    // Interesting fact that I learned by searching tutorials online on ML Agents, it is a C sharp convention
    // to name private variables with a "_" first, to diferentiate them from public ones,
    // So thanks Mohamed (my HCI professor) for the ideia! Many good things came from this project
    
    // Target that the agent must reach during the episode
    [SerializeField] private Transform _goal;
    // Renderer of the ground, used to give visual feedback (green/red flash)
    [SerializeField] private Renderer _groundRenderer;
    // Forward movement speed of the agent
    [SerializeField] private float _moveSpeed = 1.5f;
    // Angular speed for left/right rotation
    [SerializeField] private float _rotationSpeed = 180f;

    // Renderer of the agent itself (for color feedback)
    private Renderer _renderer;

    // Current episode index (for debug/GUI purposes)
    [HideInInspector] public int CurrentEpisode = 0;
    // Cumulative reward of the current episode
    [HideInInspector] public float CumulativeReward = 0f;

    // Original color of the ground before flashing
    private Color _defaultGroundColor;
    // Reference to the coroutine that flashes the ground
    private Coroutine _flashGroundCoroutine;

    // Called once when the agent is initialized
    public override void Initialize()
    {
        Debug.Log("Initialize()");

        // Cache the agent's renderer component
        _renderer = GetComponent<Renderer>();
        CurrentEpisode = 0;
        CumulativeReward = 0f;

        // If a ground renderer is assigned, store its default color
        if (_groundRenderer != null)
        { // Store default gray color of the ground plane
            _defaultGroundColor = _groundRenderer.material.color;
        }
    }

    // Called at the beginning of each training episode
    public override void OnEpisodeBegin()
    {
        Debug.Log("OnEpisodeBegin()");

        // If there was a previous episode and a ground renderer, flash green/red depending on performance
        if (_groundRenderer != null && CumulativeReward != 0f)
        {
            Color flashColor = (CumulativeReward > 0f) ? Color.green : Color.red;

            // Stop any existing FlashGround coroutine before starting a new one
            if (_flashGroundCoroutine != null)
            {
                StopCoroutine(_flashGroundCoroutine);
            }

            // Start a new coroutine to smoothly flash the ground color
            _flashGroundCoroutine = StartCoroutine(FlashGround(flashColor, 3.0f));
        }

        // Increment episode counter and reset cumulative reward
        CurrentEpisode++;
        CumulativeReward = 0f;
        // Reset agent color to blue at the start of the episode
        _renderer.material.color = Color.blue;

        // Randomize the starting configuration of agent and goal
        SpawnObjects();
    }

    // Coroutine that gradually blends the ground from a flash color back to its default
    private IEnumerator FlashGround(Color targetColor, float duration)
    {
        float elapsedTime = 0f;

        _groundRenderer.material.color = targetColor;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            _groundRenderer.material.color = Color.Lerp(targetColor, _defaultGroundColor, elapsedTime / duration);
            yield return null;
        }
    }

    // Position the agent at the center and the goal in a random direction and distance
    private void SpawnObjects()
    {
        // Reset agent rotation and position at the origin of the arena
        transform.localRotation = Quaternion.identity;
        transform.localPosition = new Vector3(0f, 0.15f, 0f);

        // Randomize the direction on the Y-axis (angle in degrees)
        float randomAngle = Random.Range(0f, 360f);
        Vector3 randomDirection = Quaternion.Euler(0f, randomAngle, 0f) * Vector3.forward;

        // Randomize the distance within the range [1, 2.5]
        float randomDistance = Random.Range(1f, 2.5f);

        // Calculate the goal's position
        Vector3 goalPosition = transform.localPosition + randomDirection * randomDistance;

        // Apply the calculated position to the goal
        _goal.localPosition = new Vector3(goalPosition.x, 0.3f, goalPosition.z);
    }

    // Collect observations that will be passed to the neural network
    public override void CollectObservations(VectorSensor sensor)
    {
        // The Goal's position
        float goalPosX_normalized = _goal.localPosition.x / 5f;
        float goalPosZ_normalized = _goal.localPosition.z / 5f;

        // The Bot's position
        float trackBotPosX_normalized = transform.localPosition.x / 5f;
        float trackBotPosZ_normalized = transform.localPosition.z / 5f;

        // The trackBot's direction (on the Y Axis)
        float trackBotRotation_normalized = (transform.localRotation.eulerAngles.y / 360f) * 2f - 1f;

        // Add goal position, agent position and agent orientation as observations
        sensor.AddObservation(goalPosX_normalized);
        sensor.AddObservation(goalPosZ_normalized);
        sensor.AddObservation(trackBotPosX_normalized);
        sensor.AddObservation(trackBotPosZ_normalized);
        sensor.AddObservation(trackBotRotation_normalized);
    }

    // Manual control to test the agent using keyboard input
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        discreteActionsOut[0] = 0; //don't move - do nothing!

        if (Input.GetKey(KeyCode.UpArrow))
        {
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            discreteActionsOut[0] = 2;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            discreteActionsOut[0] = 3;
        }
    }

    // Called every decision step with the chosen actions from the policy
    public override void OnActionReceived(ActionBuffers actions)
    {
        // Move the agent using the action.
        MoveAgent(actions.DiscreteActions);

        // Penalty given each step to encourage agent to finish task quickly.
        AddReward(-2f / MaxStep);

        // Update the cumulative reward after adding the step penalty.
        CumulativeReward = GetCumulativeReward();
    }

    // Interpret the discrete action and apply movement/rotation accordingly
    public void MoveAgent(ActionSegment<int> act)
    {
        var action = act[0];

        switch (action)
        {
            case 1: // Move forward
                transform.position += transform.forward * _moveSpeed * Time.deltaTime;
                break;
            case 2: // Rotate left
                transform.Rotate(0f, -_rotationSpeed * Time.deltaTime, 0f);
                break;
            case 3: // Rotate right
                transform.Rotate(0f, _rotationSpeed * Time.deltaTime, 0f);
                break;
        }
    }

    // Triggered when entering a trigger collider (e.g., the goal area)
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Goal"))
        {
            GoalReached();
        }
    }

    // Handle the successful event of reaching the goal
    private void GoalReached()
    {
        AddReward(1.0f); // Large reward for reaching the goal
        CumulativeReward = GetCumulativeReward();

        // End the current episode after success
        EndEpisode();
    }

    // Called when the agent first collides with another collider
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            // Apply a small negative reward when the collision starts
            AddReward(-0.05f);

            // Change the color of the trackBotAgent to red
            if (_renderer != null)
            {
                _renderer.material.color = Color.red;
            }
        }
    }

    // Called each physics step while the agent stays in contact with the collider
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            // Continually penalize the agent while it is in contact with the wall
            AddReward(-0.01f * Time.fixedDeltaTime);
        }
    }

    // Called when the agent stops colliding with the collider
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            // Reset the color when the collision ends
            if (_renderer != null)
            {
                // Assuming blue is the default color
                _renderer.material.color = Color.blue;
            }
        }
    }
}
