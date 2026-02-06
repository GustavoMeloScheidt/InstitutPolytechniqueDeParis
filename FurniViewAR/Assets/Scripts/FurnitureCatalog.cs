using System;
using UnityEngine;

/// <summary>
/// Holds the list of available furniture prefabs for placement.
/// </summary>
public class FurnitureCatalog : MonoBehaviour
{
    [Serializable]
    public class FurnitureItem
    {
        public string displayName;
        public GameObject prefab;
    }

    [SerializeField] private FurnitureItem[] furnitureItems;

    private int selectedIndex = 0;

    /// <summary>
    /// All available furniture items.
    /// </summary>
    public FurnitureItem[] Items => furnitureItems;

    /// <summary>
    /// Currently selected furniture index.
    /// </summary>
    public int SelectedIndex => selectedIndex;

    /// <summary>
    /// Currently selected furniture prefab.
    /// </summary>
    public GameObject SelectedPrefab
    {
        get
        {
            if (furnitureItems == null || furnitureItems.Length == 0) return null;
            return furnitureItems[selectedIndex].prefab;
        }
    }

    /// <summary>
    /// Fired when the selected furniture changes.
    /// Parameter: new index.
    /// </summary>
    public event Action<int> OnSelectionChanged;

    /// <summary>
    /// Selects a furniture item by index.
    /// </summary>
    public void Select(int index)
    {
        if (furnitureItems == null || index < 0 || index >= furnitureItems.Length) return;

        selectedIndex = index;
        OnSelectionChanged?.Invoke(selectedIndex);
    }
}
