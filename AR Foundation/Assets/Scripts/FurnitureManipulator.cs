using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Applies gesture inputs to the selected furniture.
/// Handles movement along planes, scaling, and rotation.
/// </summary>
public class FurnitureManipulator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARAnchorManager anchorManager;
    [SerializeField] private FurnitureGestureHandler gestureHandler;

    [Header("Manipulation Settings")]
    [SerializeField] private float moveSmoothing = 10f;
    [SerializeField] private float reAnchorDistance = 0.5f;

    private PlacedFurniture selectedFurniture;
    private Vector3 targetPosition;
    private bool isMoving;
    private Vector3 lastAnchoredPosition;

    private static readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private Camera arCamera;

    /// <summary>
    /// The currently selected furniture being manipulated.
    /// </summary>
    public PlacedFurniture SelectedFurniture => selectedFurniture;

    private void Start()
    {
        arCamera = Camera.main;

        if (raycastManager == null)
        {
            raycastManager = FindFirstObjectByType<ARRaycastManager>();
        }

        if (anchorManager == null)
        {
            anchorManager = FindFirstObjectByType<ARAnchorManager>();
        }

        if (gestureHandler == null)
        {
            gestureHandler = GetComponent<FurnitureGestureHandler>();
        }
    }

    private void OnEnable()
    {
        if (gestureHandler != null)
        {
            gestureHandler.OnDragStart += HandleDragStart;
            gestureHandler.OnDragUpdate += HandleDragUpdate;
            gestureHandler.OnDragEnd += HandleDragEnd;
            gestureHandler.OnPinchScale += HandlePinchScale;
            gestureHandler.OnRotate += HandleRotate;
        }
    }

    private void OnDisable()
    {
        if (gestureHandler != null)
        {
            gestureHandler.OnDragStart -= HandleDragStart;
            gestureHandler.OnDragUpdate -= HandleDragUpdate;
            gestureHandler.OnDragEnd -= HandleDragEnd;
            gestureHandler.OnPinchScale -= HandlePinchScale;
            gestureHandler.OnRotate -= HandleRotate;
        }
    }

    private void Update()
    {
        if (isMoving && selectedFurniture != null)
        {
            // Smooth movement towards target
            Vector3 currentPos = selectedFurniture.transform.position;
            Vector3 newPos = Vector3.Lerp(currentPos, targetPosition, Time.deltaTime * moveSmoothing);
            selectedFurniture.UpdatePosition(newPos);
        }
    }

    /// <summary>
    /// Sets the furniture to be manipulated.
    /// </summary>
    public void SetSelectedFurniture(PlacedFurniture furniture)
    {
        selectedFurniture = furniture;
        if (furniture != null)
        {
            lastAnchoredPosition = furniture.transform.position;
        }
    }

    /// <summary>
    /// Clears the selected furniture.
    /// </summary>
    public void ClearSelection()
    {
        selectedFurniture = null;
        isMoving = false;
    }

    private void HandleDragStart(Vector2 screenPosition)
    {
        if (selectedFurniture == null) return;

        isMoving = true;
        targetPosition = selectedFurniture.transform.position;
    }

    private void HandleDragUpdate(Vector2 screenPosition, Vector2 delta)
    {
        if (selectedFurniture == null || !isMoving) return;

        // Raycast to find new position on horizontal plane
        if (raycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            foreach (var hit in hits)
            {
                var plane = hit.trackable as ARPlane;
                if (plane != null && plane.alignment == PlaneAlignment.HorizontalUp)
                {
                    targetPosition = hit.pose.position;
                    break;
                }
            }
        }
    }

    private void HandleDragEnd()
    {
        if (selectedFurniture == null) return;

        isMoving = false;

        // Check if we need to re-anchor
        float distanceMoved = Vector3.Distance(selectedFurniture.transform.position, lastAnchoredPosition);
        if (distanceMoved > reAnchorDistance)
        {
            ReAnchorFurniture();
        }
    }

    private void ReAnchorFurniture()
    {
        if (selectedFurniture == null || anchorManager == null) return;

        Vector2 screenPos = arCamera.WorldToScreenPoint(selectedFurniture.transform.position);

        if (raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
        {
            foreach (var hit in hits)
            {
                var plane = hit.trackable as ARPlane;
                if (plane != null && plane.alignment == PlaneAlignment.HorizontalUp)
                {
                    // Remove old anchor
                    if (selectedFurniture.Anchor != null)
                    {
                        Destroy(selectedFurniture.Anchor.gameObject);
                    }

                    // Create new anchor at current position
                    Pose currentPose = new Pose(
                        selectedFurniture.transform.position,
                        selectedFurniture.transform.rotation
                    );

                    ARAnchor newAnchor = anchorManager.AttachAnchor(plane, currentPose);
                    if (newAnchor != null)
                    {
                        selectedFurniture.ReAnchor(newAnchor);
                        lastAnchoredPosition = selectedFurniture.transform.position;
                    }
                    break;
                }
            }
        }
    }

    private void HandlePinchScale(float scaleDelta)
    {
        if (selectedFurniture == null) return;

        selectedFurniture.UpdateScale(scaleDelta);
    }

    private void HandleRotate(float rotationDelta)
    {
        if (selectedFurniture == null) return;

        selectedFurniture.UpdateRotation(rotationDelta);
    }
}
