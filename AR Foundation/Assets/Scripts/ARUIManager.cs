using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages AR UI elements including instruction text, delete button, and scanning indicator.
/// </summary>
public class ARUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private Button deleteButton;
    [SerializeField] private GameObject scanningIndicator;

    [Header("Instruction Messages")]
    [SerializeField] private string scanningMessage = "Move your device to scan the floor";
    [SerializeField] private string readyToPlaceMessage = "Tap to place furniture";
    [SerializeField] private string selectedMessage = "Drag to move, pinch to scale, rotate with two fingers";

    public enum UIState
    {
        Scanning,
        ReadyToPlace,
        Selected
    }

    private UIState currentState = UIState.Scanning;

    /// <summary>
    /// Event fired when the delete button is pressed.
    /// </summary>
    public event System.Action OnDeletePressed;

    private void Start()
    {
        if (deleteButton != null)
        {
            deleteButton.onClick.AddListener(HandleDeletePressed);
            deleteButton.gameObject.SetActive(false);
        }

        SetState(UIState.Scanning);
    }

    private void OnDestroy()
    {
        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveListener(HandleDeletePressed);
        }
    }

    /// <summary>
    /// Sets the current UI state and updates visuals accordingly.
    /// </summary>
    public void SetState(UIState state)
    {
        currentState = state;

        switch (state)
        {
            case UIState.Scanning:
                SetInstructionText(scanningMessage);
                SetScanningIndicatorVisible(true);
                SetDeleteButtonVisible(false);
                break;

            case UIState.ReadyToPlace:
                SetInstructionText(readyToPlaceMessage);
                SetScanningIndicatorVisible(false);
                SetDeleteButtonVisible(false);
                break;

            case UIState.Selected:
                SetInstructionText(selectedMessage);
                SetScanningIndicatorVisible(false);
                SetDeleteButtonVisible(true);
                break;
        }
    }

    /// <summary>
    /// Gets the current UI state.
    /// </summary>
    public UIState GetState()
    {
        return currentState;
    }

    /// <summary>
    /// Sets the instruction text directly.
    /// </summary>
    public void SetInstructionText(string text)
    {
        if (instructionText != null)
        {
            instructionText.text = text;
        }
    }

    /// <summary>
    /// Shows or hides the scanning indicator.
    /// </summary>
    public void SetScanningIndicatorVisible(bool visible)
    {
        if (scanningIndicator != null)
        {
            scanningIndicator.SetActive(visible);
        }
    }

    /// <summary>
    /// Shows or hides the delete button.
    /// </summary>
    public void SetDeleteButtonVisible(bool visible)
    {
        if (deleteButton != null)
        {
            deleteButton.gameObject.SetActive(visible);
        }
    }

    private void HandleDeletePressed()
    {
        OnDeletePressed?.Invoke();
    }
}
