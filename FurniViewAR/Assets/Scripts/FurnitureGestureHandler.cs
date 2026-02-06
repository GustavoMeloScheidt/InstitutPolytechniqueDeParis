using System;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

/// <summary>
/// Touch gesture detection using EnhancedTouch API.
/// Detects tap, drag, pinch scale, and rotation gestures.
/// </summary>
public class FurnitureGestureHandler : MonoBehaviour
{
    [Header("Gesture Settings")]
    [SerializeField] private float tapMaxDuration = 0.3f;
    [SerializeField] private float tapMaxDistance = 20f;
    [SerializeField] private float pinchSensitivity = 0.005f;
    [SerializeField] private float rotationSensitivity = 0.5f;
    [SerializeField] private float dragThreshold = 10f;

    // Tap tracking
    private bool isTapping;
    private float tapStartTime;
    private Vector2 tapStartPosition;

    // Two-finger gesture tracking
    private float previousPinchDistance;
    private float previousRotationAngle;
    private bool isTwoFingerGesture;

    // Drag tracking
    private bool isDragging;
    private Vector2 previousDragPosition;

    /// <summary>
    /// Fired when a tap gesture is detected.
    /// Parameter: screen position of the tap.
    /// </summary>
    public event Action<Vector2> OnTap;

    /// <summary>
    /// Fired during a drag gesture.
    /// Parameters: current position, delta from previous frame.
    /// </summary>
    public event Action<Vector2, Vector2> OnDragUpdate;

    /// <summary>
    /// Fired when drag gesture starts.
    /// Parameter: starting screen position.
    /// </summary>
    public event Action<Vector2> OnDragStart;

    /// <summary>
    /// Fired when drag gesture ends.
    /// </summary>
    public event Action OnDragEnd;

    /// <summary>
    /// Fired during a pinch gesture.
    /// Parameter: scale delta (positive = zoom in, negative = zoom out).
    /// </summary>
    public event Action<float> OnPinchScale;

    /// <summary>
    /// Fired during a rotation gesture.
    /// Parameter: rotation delta in degrees.
    /// </summary>
    public event Action<float> OnRotate;

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        int touchCount = Touch.activeTouches.Count;

        if (touchCount == 0)
        {
            ResetGestureState();
            return;
        }

        if (touchCount == 1)
        {
            HandleSingleTouch(Touch.activeTouches[0]);
        }
        else if (touchCount >= 2)
        {
            HandleTwoFingerTouch(Touch.activeTouches[0], Touch.activeTouches[1]);
        }
    }

    private void HandleSingleTouch(Touch touch)
    {
        // If we were doing a two-finger gesture, don't process single touch
        if (isTwoFingerGesture)
        {
            isTwoFingerGesture = false;
            return;
        }

        switch (touch.phase)
        {
            case TouchPhase.Began:
                isTapping = true;
                tapStartTime = Time.time;
                tapStartPosition = touch.screenPosition;
                isDragging = false;
                previousDragPosition = touch.screenPosition;
                break;

            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                Vector2 currentPosition = touch.screenPosition;
                float distanceFromStart = Vector2.Distance(currentPosition, tapStartPosition);

                // Check if we should start dragging
                if (!isDragging && distanceFromStart > dragThreshold)
                {
                    isDragging = true;
                    isTapping = false;
                    OnDragStart?.Invoke(currentPosition);
                }

                if (isDragging)
                {
                    Vector2 delta = currentPosition - previousDragPosition;
                    if (delta.sqrMagnitude > 0.01f)
                    {
                        OnDragUpdate?.Invoke(currentPosition, delta);
                    }
                    previousDragPosition = currentPosition;
                }
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                if (isTapping)
                {
                    float duration = Time.time - tapStartTime;
                    float distance = Vector2.Distance(touch.screenPosition, tapStartPosition);

                    if (duration <= tapMaxDuration && distance <= tapMaxDistance)
                    {
                        OnTap?.Invoke(touch.screenPosition);
                    }
                }

                if (isDragging)
                {
                    OnDragEnd?.Invoke();
                }

                isTapping = false;
                isDragging = false;
                break;
        }
    }

    private void HandleTwoFingerTouch(Touch touch0, Touch touch1)
    {
        // Cancel any single touch gestures
        if (isTapping)
        {
            isTapping = false;
        }
        if (isDragging)
        {
            OnDragEnd?.Invoke();
            isDragging = false;
        }

        Vector2 pos0 = touch0.screenPosition;
        Vector2 pos1 = touch1.screenPosition;

        float currentDistance = Vector2.Distance(pos0, pos1);
        float currentAngle = GetAngleBetweenTouches(pos0, pos1);

        // Initialize on first two-finger touch
        if (!isTwoFingerGesture)
        {
            isTwoFingerGesture = true;
            previousPinchDistance = currentDistance;
            previousRotationAngle = currentAngle;
            return;
        }

        // Calculate pinch delta
        float pinchDelta = (currentDistance - previousPinchDistance) * pinchSensitivity;
        if (Mathf.Abs(pinchDelta) > 0.001f)
        {
            OnPinchScale?.Invoke(pinchDelta);
        }

        // Calculate rotation delta
        float rotationDelta = Mathf.DeltaAngle(previousRotationAngle, currentAngle) * rotationSensitivity;
        if (Mathf.Abs(rotationDelta) > 0.1f)
        {
            OnRotate?.Invoke(rotationDelta);
        }

        previousPinchDistance = currentDistance;
        previousRotationAngle = currentAngle;
    }

    private float GetAngleBetweenTouches(Vector2 pos0, Vector2 pos1)
    {
        Vector2 direction = pos1 - pos0;
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    private void ResetGestureState()
    {
        if (isDragging)
        {
            OnDragEnd?.Invoke();
        }

        isTapping = false;
        isDragging = false;
        isTwoFingerGesture = false;
    }

    /// <summary>
    /// Checks if a touch is over UI elements.
    /// </summary>
    public static bool IsTouchOverUI(Vector2 screenPosition)
    {
        return UnityEngine.EventSystems.EventSystem.current != null &&
               UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }
}
