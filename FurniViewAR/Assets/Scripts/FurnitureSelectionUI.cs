using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Creates and manages a horizontal scrollable furniture selection panel.
/// Dynamically generates buttons from the FurnitureCatalog.
/// </summary>
public class FurnitureSelectionUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FurnitureCatalog catalog;
    [SerializeField] private RectTransform buttonContainer;
    [SerializeField] private GameObject buttonPrefab;

    [Header("Button Appearance")]
    [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private Color selectedColor = new Color(0.2f, 0.6f, 1f, 0.9f);
    [SerializeField] private float buttonWidth = 140f;
    [SerializeField] private float buttonHeight = 50f;
    [SerializeField] private float buttonSpacing = 10f;

    private Button[] buttons;
    private Image[] buttonImages;
    private int currentSelection = 0;

    private void Start()
    {
        GenerateButtons();

        if (catalog != null)
        {
            catalog.OnSelectionChanged += UpdateButtonVisuals;
        }
    }

    private void OnDestroy()
    {
        if (catalog != null)
        {
            catalog.OnSelectionChanged -= UpdateButtonVisuals;
        }
    }

    private void GenerateButtons()
    {
        if (catalog == null || catalog.Items == null || catalog.Items.Length == 0) return;

        // If a button prefab is provided, use it; otherwise create buttons from scratch
        int count = catalog.Items.Length;
        buttons = new Button[count];
        buttonImages = new Image[count];

        // Set container size
        float totalWidth = count * (buttonWidth + buttonSpacing) - buttonSpacing;
        buttonContainer.sizeDelta = new Vector2(totalWidth, buttonHeight);

        for (int i = 0; i < count; i++)
        {
            GameObject btnObj;

            if (buttonPrefab != null)
            {
                btnObj = Instantiate(buttonPrefab, buttonContainer);
            }
            else
            {
                btnObj = CreateDefaultButton();
                btnObj.transform.SetParent(buttonContainer, false);
            }

            btnObj.name = "Btn_" + catalog.Items[i].displayName;

            // Set up RectTransform
            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(buttonWidth, buttonHeight);

            // Set up button text
            TextMeshProUGUI label = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = catalog.Items[i].displayName;
                label.fontSize = 16;
                label.alignment = TextAlignmentOptions.Center;
                label.color = Color.white;
            }
            else
            {
                // Fallback to legacy Text
                Text legacyLabel = btnObj.GetComponentInChildren<Text>();
                if (legacyLabel != null)
                {
                    legacyLabel.text = catalog.Items[i].displayName;
                }
            }

            // Store references
            buttons[i] = btnObj.GetComponent<Button>();
            buttonImages[i] = btnObj.GetComponent<Image>();

            // Set up click handler
            int index = i; // Capture for closure
            buttons[i].onClick.AddListener(() => OnButtonClicked(index));
        }

        // Highlight the first button
        UpdateButtonVisuals(0);
    }

    private GameObject CreateDefaultButton()
    {
        GameObject btnObj = new GameObject("FurnitureButton", typeof(RectTransform), typeof(Image), typeof(Button));

        Image img = btnObj.GetComponent<Image>();
        img.color = normalColor;

        // Add rounded corners feel
        Button btn = btnObj.GetComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 0.9f);
        colors.pressedColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);
        btn.colors = colors;

        // Create text child
        GameObject textObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(btnObj.transform, false);

        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(5, 2);
        textRt.offsetMax = new Vector2(-5, -2);

        return btnObj;
    }

    private void OnButtonClicked(int index)
    {
        if (catalog != null)
        {
            catalog.Select(index);
        }
    }

    private void UpdateButtonVisuals(int selectedIndex)
    {
        currentSelection = selectedIndex;

        if (buttonImages == null) return;

        for (int i = 0; i < buttonImages.Length; i++)
        {
            if (buttonImages[i] != null)
            {
                buttonImages[i].color = (i == selectedIndex) ? selectedColor : normalColor;
            }
        }
    }

    /// <summary>
    /// Shows or hides the selection panel.
    /// </summary>
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
