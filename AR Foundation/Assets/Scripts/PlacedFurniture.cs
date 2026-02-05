using UnityEngine;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// Component attached to each placed furniture instance.
/// Handles selection state, visual feedback, and manipulation.
/// </summary>
public class PlacedFurniture : MonoBehaviour
{
    [Header("Selection Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = new Color(0.5f, 0.8f, 1f, 1f);

    [Header("Scale Constraints")]
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 2.0f;

    private ARAnchor anchor;
    private Renderer[] renderers;
    private MaterialPropertyBlock propertyBlock;
    private Vector3 initialScale;

    /// <summary>
    /// Whether this furniture is currently selected.
    /// </summary>
    public bool IsSelected { get; private set; }

    /// <summary>
    /// The AR anchor this furniture is attached to.
    /// </summary>
    public ARAnchor Anchor => anchor;

    /// <summary>
    /// Current scale multiplier relative to initial scale.
    /// </summary>
    public float CurrentScaleMultiplier { get; private set; } = 1f;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
        initialScale = transform.localScale;
    }

    /// <summary>
    /// Initializes the furniture with an AR anchor.
    /// </summary>
    public void Initialize(ARAnchor attachedAnchor)
    {
        anchor = attachedAnchor;
        if (anchor != null)
        {
            transform.SetParent(anchor.transform, true);
        }
    }

    /// <summary>
    /// Selects this furniture and shows visual feedback.
    /// </summary>
    public void Select()
    {
        if (IsSelected) return;

        IsSelected = true;
        UpdateSelectionVisual();
    }

    /// <summary>
    /// Deselects this furniture and removes visual feedback.
    /// </summary>
    public void Deselect()
    {
        if (!IsSelected) return;

        IsSelected = false;
        UpdateSelectionVisual();
    }

    private void UpdateSelectionVisual()
    {
        Color targetColor = IsSelected ? selectedColor : normalColor;

        foreach (var renderer in renderers)
        {
            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_BaseColor", targetColor);
            propertyBlock.SetColor("_Color", targetColor);
            renderer.SetPropertyBlock(propertyBlock);
        }
    }

    /// <summary>
    /// Updates the furniture position on the AR plane.
    /// </summary>
    public void UpdatePosition(Vector3 newPosition)
    {
        if (anchor != null)
        {
            // Keep relative position to anchor or move directly
            transform.position = newPosition;
        }
        else
        {
            transform.position = newPosition;
        }
    }

    /// <summary>
    /// Updates the furniture scale with clamping.
    /// </summary>
    public void UpdateScale(float scaleDelta)
    {
        CurrentScaleMultiplier = Mathf.Clamp(CurrentScaleMultiplier + scaleDelta, minScale, maxScale);
        transform.localScale = initialScale * CurrentScaleMultiplier;
    }

    /// <summary>
    /// Sets the absolute scale multiplier.
    /// </summary>
    public void SetScale(float scaleMultiplier)
    {
        CurrentScaleMultiplier = Mathf.Clamp(scaleMultiplier, minScale, maxScale);
        transform.localScale = initialScale * CurrentScaleMultiplier;
    }

    /// <summary>
    /// Updates the furniture rotation around the Y-axis.
    /// </summary>
    public void UpdateRotation(float angleDelta)
    {
        transform.Rotate(Vector3.up, angleDelta, Space.World);
    }

    /// <summary>
    /// Sets the absolute Y-axis rotation.
    /// </summary>
    public void SetRotation(float yAngle)
    {
        Vector3 euler = transform.eulerAngles;
        euler.y = yAngle;
        transform.eulerAngles = euler;
    }

    /// <summary>
    /// Re-anchors the furniture to a new anchor.
    /// </summary>
    public void ReAnchor(ARAnchor newAnchor)
    {
        if (anchor != null)
        {
            // Detach from old anchor
            transform.SetParent(null, true);
        }

        anchor = newAnchor;

        if (anchor != null)
        {
            transform.SetParent(anchor.transform, true);
        }
    }

    /// <summary>
    /// Destroys this furniture and its anchor.
    /// </summary>
    public void Remove()
    {
        if (anchor != null)
        {
            Destroy(anchor.gameObject);
        }
        Destroy(gameObject);
    }
}
