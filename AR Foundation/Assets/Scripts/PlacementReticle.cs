using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Visual indicator showing where furniture will be placed.
/// Continuously raycasts from screen center to horizontal planes.
/// </summary>
public class PlacementReticle : MonoBehaviour
{
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private GameObject reticleVisual;

    private static readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private Camera arCamera;
    private Renderer reticleRenderer;

    /// <summary>
    /// The pose where furniture should be placed.
    /// </summary>
    public Pose PlacementPose { get; private set; }

    /// <summary>
    /// Whether the reticle is on a valid placement surface.
    /// </summary>
    public bool IsValidPlacement { get; private set; }

    /// <summary>
    /// The AR plane the reticle is currently on.
    /// </summary>
    public ARPlane CurrentPlane { get; private set; }

    private void Start()
    {
        arCamera = Camera.main;

        if (raycastManager == null)
        {
            raycastManager = FindFirstObjectByType<ARRaycastManager>();
        }

        // Get renderer to toggle visibility without deactivating the GameObject
        if (reticleVisual != null)
        {
            reticleRenderer = reticleVisual.GetComponent<Renderer>();
            if (reticleRenderer == null)
            {
                reticleRenderer = reticleVisual.GetComponentInChildren<Renderer>();
            }
        }

        SetReticleVisible(false);
    }

    private void Update()
    {
        UpdateReticlePose();
    }

    private void UpdateReticlePose()
    {
        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        // Raycast only against horizontal planes
        if (raycastManager != null &&
            raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            // Filter for horizontal planes only
            ARRaycastHit? validHit = null;
            foreach (var hit in hits)
            {
                var plane = hit.trackable as ARPlane;
                if (plane != null && plane.alignment == PlaneAlignment.HorizontalUp)
                {
                    validHit = hit;
                    break;
                }
            }

            if (validHit.HasValue)
            {
                var hitPose = validHit.Value.pose;

                // Calculate rotation to face camera (Y-axis only)
                Vector3 cameraForward = arCamera.transform.forward;
                cameraForward.y = 0;
                cameraForward.Normalize();

                Quaternion targetRotation = Quaternion.LookRotation(cameraForward, Vector3.up);

                PlacementPose = new Pose(hitPose.position, targetRotation);
                CurrentPlane = validHit.Value.trackable as ARPlane;
                IsValidPlacement = true;

                // Update reticle transform
                transform.position = PlacementPose.position;
                transform.rotation = PlacementPose.rotation;

                SetReticleVisible(true);
            }
            else
            {
                SetInvalid();
            }
        }
        else
        {
            SetInvalid();
        }
    }

    private void SetInvalid()
    {
        IsValidPlacement = false;
        CurrentPlane = null;
        SetReticleVisible(false);
    }

    /// <summary>
    /// Shows or hides the reticle.
    /// </summary>
    public void SetVisible(bool visible)
    {
        SetReticleVisible(visible && IsValidPlacement);
    }

    private void SetReticleVisible(bool visible)
    {
        if (reticleRenderer != null)
        {
            reticleRenderer.enabled = visible;
        }
    }
}
