using UnityEngine;
using UnityEngine.EventSystems;

public class DragDialPad_S : MonoBehaviour,
    IPointerDownHandler, IDragHandler
{
    [Header("Dial Pad Root")]
    public RectTransform dialPadRoot; // whole dial pad panel

    Vector2 pointerOffset;

    public void OnPointerDown(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dialPadRoot,
            eventData.position,
            eventData.pressEventCamera,
            out pointerOffset
        );
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dialPadRoot.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        dialPadRoot.localPosition = localPoint - pointerOffset;
    }
}
