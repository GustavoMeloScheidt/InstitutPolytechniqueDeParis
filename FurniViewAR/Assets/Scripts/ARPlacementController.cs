using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Main controller orchestrating the AR furniture placement workflow.
/// Manages state machine, tap-to-place, and furniture selection.
/// </summary>
public class ARPlacementController : MonoBehaviour
{
    public enum PlacementState
    {
        Scanning,
        ReadyToPlace,
        Manipulating
    }

    [Header("AR References")]
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARAnchorManager anchorManager;
    [SerializeField] private ARPlaneManager planeManager;

    [Header("Placement Components")]
    [SerializeField] private PlacementReticle placementReticle;
    [SerializeField] private FurnitureGestureHandler gestureHandler;
    [SerializeField] private FurnitureManipulator furnitureManipulator;

    [Header("UI")]
    [SerializeField] private ARUIManager uiManager;

    [Header("Furniture")]
    [SerializeField] private FurnitureCatalog furnitureCatalog;
    [SerializeField] private LayerMask furnitureLayerMask = -1;
    [SerializeField] private int furnitureLayer = 6;

    [Header("Settings")]
    [SerializeField] private float selectionRaycastDistance = 100f;

    private PlacementState currentState = PlacementState.Scanning;
    private PlacedFurniture selectedFurniture;
    private List<PlacedFurniture> placedFurnitureList = new List<PlacedFurniture>();
    private Camera arCamera;

    private static readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();

    /// <summary>
    /// Current placement state.
    /// </summary>
    public PlacementState CurrentState => currentState;

    /// <summary>
    /// Currently selected furniture, if any.
    /// </summary>
    public PlacedFurniture SelectedFurniture => selectedFurniture;

    private void Start()
    {
        arCamera = Camera.main;

        // Auto-find references if not set
        if (raycastManager == null)
            raycastManager = FindFirstObjectByType<ARRaycastManager>();
        if (anchorManager == null)
            anchorManager = FindFirstObjectByType<ARAnchorManager>();
        if (planeManager == null)
            planeManager = FindFirstObjectByType<ARPlaneManager>();

        // Set initial state
        SetState(PlacementState.Scanning);
    }

    private void OnEnable()
    {
        if (gestureHandler != null)
        {
            gestureHandler.OnTap += HandleTap;
        }

        if (uiManager != null)
        {
            uiManager.OnDeletePressed += HandleDeletePressed;
        }
    }

    private void OnDisable()
    {
        if (gestureHandler != null)
        {
            gestureHandler.OnTap -= HandleTap;
        }

        if (uiManager != null)
        {
            uiManager.OnDeletePressed -= HandleDeletePressed;
        }
    }

    private void Update()
    {
        UpdateStateMachine();
    }

    private void UpdateStateMachine()
    {
        switch (currentState)
        {
            case PlacementState.Scanning:
                // Transition to ReadyToPlace when reticle finds valid surface
                if (placementReticle != null && placementReticle.IsValidPlacement)
                {
                    SetState(PlacementState.ReadyToPlace);
                }
                break;

            case PlacementState.ReadyToPlace:
                // Transition back to Scanning if reticle loses valid surface
                if (placementReticle != null && !placementReticle.IsValidPlacement)
                {
                    SetState(PlacementState.Scanning);
                }
                break;

            case PlacementState.Manipulating:
                // Stay in manipulating state while furniture is selected
                if (selectedFurniture == null)
                {
                    // Transition based on reticle validity
                    if (placementReticle != null && placementReticle.IsValidPlacement)
                    {
                        SetState(PlacementState.ReadyToPlace);
                    }
                    else
                    {
                        SetState(PlacementState.Scanning);
                    }
                }
                break;
        }
    }

    private void SetState(PlacementState newState)
    {
        currentState = newState;

        // Update reticle visibility
        if (placementReticle != null)
        {
            bool showReticle = (newState == PlacementState.ReadyToPlace);
            placementReticle.SetVisible(showReticle);
        }

        // Update UI
        if (uiManager != null)
        {
            switch (newState)
            {
                case PlacementState.Scanning:
                    uiManager.SetState(ARUIManager.UIState.Scanning);
                    break;
                case PlacementState.ReadyToPlace:
                    uiManager.SetState(ARUIManager.UIState.ReadyToPlace);
                    break;
                case PlacementState.Manipulating:
                    uiManager.SetState(ARUIManager.UIState.Selected);
                    break;
            }
        }
    }

    private void HandleTap(Vector2 screenPosition)
    {
        // Ignore taps over UI
        if (FurnitureGestureHandler.IsTouchOverUI(screenPosition))
        {
            return;
        }

        // First, try to select/deselect furniture
        if (TrySelectFurniture(screenPosition))
        {
            return;
        }

        // If we have selected furniture and tapped elsewhere, deselect
        if (selectedFurniture != null)
        {
            DeselectFurniture();
            return;
        }

        // If no furniture hit and reticle is valid, place new furniture
        if (currentState == PlacementState.ReadyToPlace &&
            placementReticle != null &&
            placementReticle.IsValidPlacement)
        {
            PlaceFurniture();
        }
    }

    private bool TrySelectFurniture(Vector2 screenPosition)
    {
        Ray ray = arCamera.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, selectionRaycastDistance, furnitureLayerMask))
        {
            PlacedFurniture furniture = hit.collider.GetComponentInParent<PlacedFurniture>();

            if (furniture != null)
            {
                if (furniture == selectedFurniture)
                {
                    // Tapping already selected furniture deselects it
                    DeselectFurniture();
                }
                else
                {
                    // Select new furniture
                    SelectFurniture(furniture);
                }
                return true;
            }
        }

        return false;
    }

    private void SelectFurniture(PlacedFurniture furniture)
    {
        // Deselect previous
        if (selectedFurniture != null)
        {
            selectedFurniture.Deselect();
        }

        selectedFurniture = furniture;
        selectedFurniture.Select();

        // Update manipulator
        if (furnitureManipulator != null)
        {
            furnitureManipulator.SetSelectedFurniture(selectedFurniture);
        }

        SetState(PlacementState.Manipulating);
    }

    private void DeselectFurniture()
    {
        if (selectedFurniture != null)
        {
            selectedFurniture.Deselect();
            selectedFurniture = null;
        }

        if (furnitureManipulator != null)
        {
            furnitureManipulator.ClearSelection();
        }

        // Transition state based on reticle
        if (placementReticle != null && placementReticle.IsValidPlacement)
        {
            SetState(PlacementState.ReadyToPlace);
        }
        else
        {
            SetState(PlacementState.Scanning);
        }
    }

    private void PlaceFurniture()
    {
        GameObject prefab = furnitureCatalog != null ? furnitureCatalog.SelectedPrefab : null;

        if (prefab == null || placementReticle == null)
        {
            Debug.LogWarning("Cannot place furniture: missing prefab or reticle");
            return;
        }

        Pose placementPose = placementReticle.PlacementPose;
        ARPlane targetPlane = placementReticle.CurrentPlane;

        // Instantiate furniture
        GameObject furnitureObj = Instantiate(prefab, placementPose.position, placementPose.rotation);

        // Ensure the furniture has a PlacedFurniture component
        PlacedFurniture furniture = furnitureObj.GetComponent<PlacedFurniture>();
        if (furniture == null)
        {
            furniture = furnitureObj.AddComponent<PlacedFurniture>();
        }

        // Ensure there's a collider for selection raycasting
        if (furnitureObj.GetComponentInChildren<Collider>() == null)
        {
            // Add a BoxCollider that encompasses all renderers
            Renderer[] renderers = furnitureObj.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }

                BoxCollider col = furnitureObj.AddComponent<BoxCollider>();
                col.center = furnitureObj.transform.InverseTransformPoint(bounds.center);
                col.size = furnitureObj.transform.InverseTransformVector(bounds.size);
                // Make size absolute (InverseTransformVector can produce negative values)
                col.size = new Vector3(
                    Mathf.Abs(col.size.x),
                    Mathf.Abs(col.size.y),
                    Mathf.Abs(col.size.z)
                );
            }
        }

        // Set layer for raycast filtering
        SetLayerRecursive(furnitureObj, furnitureLayer);

        // Create anchor if possible
        ARAnchor anchor = null;
        if (anchorManager != null && targetPlane != null)
        {
            anchor = anchorManager.AttachAnchor(targetPlane, placementPose);
        }

        furniture.Initialize(anchor);
        placedFurnitureList.Add(furniture);

        // Auto-select the newly placed furniture
        SelectFurniture(furniture);
    }

    private void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }

    private void HandleDeletePressed()
    {
        if (selectedFurniture != null)
        {
            DeleteSelectedFurniture();
        }
    }

    private void DeleteSelectedFurniture()
    {
        if (selectedFurniture == null) return;

        placedFurnitureList.Remove(selectedFurniture);

        if (furnitureManipulator != null)
        {
            furnitureManipulator.ClearSelection();
        }

        selectedFurniture.Remove();
        selectedFurniture = null;

        // Transition state
        if (placementReticle != null && placementReticle.IsValidPlacement)
        {
            SetState(PlacementState.ReadyToPlace);
        }
        else
        {
            SetState(PlacementState.Scanning);
        }
    }

    /// <summary>
    /// Removes all placed furniture from the scene.
    /// </summary>
    public void ClearAllFurniture()
    {
        foreach (var furniture in placedFurnitureList)
        {
            if (furniture != null)
            {
                furniture.Remove();
            }
        }

        placedFurnitureList.Clear();
        selectedFurniture = null;

        if (furnitureManipulator != null)
        {
            furnitureManipulator.ClearSelection();
        }

        SetState(PlacementState.Scanning);
    }
}
