using UnityEngine;
using UnityEngine.EventSystems;

public class JoyStick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public RectTransform background;
    public RectTransform handle;

    Vector2 input;

    void Start()
    {
        if (background == null) background = GetComponent<RectTransform>();
        if (handle == null) handle = transform.GetChild(0).GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        input = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background, eventData.position, eventData.pressEventCamera, out pos
        );

        // Normaliza para range [-1, 1]
        pos /= background.sizeDelta / 2f;

        input = (pos.magnitude > 1) ? pos.normalized : pos;

        handle.anchoredPosition = input * (background.sizeDelta / 2f);
    }

    public Vector2 GetDirection()
    {
        return input;
    }
}
