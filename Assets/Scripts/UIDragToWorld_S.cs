using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragToWorld_S : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Camera mainCamera;

    [Header("Snap Target")]
    public Transform targetPoint;     // Drag your snap target here

    [Header("Text To Enable")]
    public GameObject textObject;     // Drag the Text object here

    [Header("Snap Distance")]
    public float snapDistance = 1.5f;

    private Vector2 startPosition;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        mainCamera = Camera.main;

        startPosition = rectTransform.anchoredPosition;

        // Make sure text starts hidden
        if (textObject != null)
            textObject.SetActive(false);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition +=
            eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        TrySnap(eventData.position);
    }

    void TrySnap(Vector2 screenPosition)
    {
        if (targetPoint == null)
        {
            rectTransform.anchoredPosition = startPosition;
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        Plane plane = new Plane(Vector3.up, targetPoint.position);

        float distance;

        if (plane.Raycast(ray, out distance))
        {
            Vector3 dropPosition = ray.GetPoint(distance);
            float dist = Vector3.Distance(dropPosition, targetPoint.position);

            if (dist <= snapDistance)
            {
                // Hide OUTline
                gameObject.SetActive(false);

                // Show Text inside Image
                if (textObject != null)
                    textObject.SetActive(true);
            }
            else
            {
                // Not near target → reset position
                rectTransform.anchoredPosition = startPosition;
            }
        }
        else
        {
            // If raycast fails → reset position
            rectTransform.anchoredPosition = startPosition;
        }
    }
}
