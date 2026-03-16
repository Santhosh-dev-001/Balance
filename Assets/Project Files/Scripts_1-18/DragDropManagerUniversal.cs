using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class DragDropManagerUniversal : MonoBehaviour
{
    private Camera cam;
    private Transform draggedObject;
    private Vector3 offset;
    private float objectDepth;

    private Vector3 startPos;
    private Quaternion startRot;

    [Header("Tags")]
    public string draggableTag = "Draggable";
    public string targetTag = "DropTarget";

    [Header("Drop Settings")]
    public float snapDistance = 1.5f;

    [Header("Events")]
    public UnityEvent OnDroppedCorrectly;

    private bool isPointerHeld;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (Pointer.current == null) return;

        if (Pointer.current.press.wasPressedThisFrame)
            BeginDrag();

        if (Pointer.current.press.isPressed)
            Drag();

        if (Pointer.current.press.wasReleasedThisFrame)
            EndDrag();
    }

    void BeginDrag()
    {
        Vector2 screenPos = Pointer.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform.CompareTag(draggableTag))
        {
            draggedObject = hit.transform;
            startPos = draggedObject.position;
            startRot = draggedObject.rotation;

            objectDepth = cam.WorldToScreenPoint(draggedObject.position).z;
            offset = draggedObject.position - GetPointerWorldPos();

            isPointerHeld = true;
        }
    }

    void Drag()
    {
        if (!isPointerHeld || draggedObject == null) return;

        draggedObject.position = GetPointerWorldPos() + offset;
    }

    void EndDrag()
    {
        if (!isPointerHeld || draggedObject == null) return;

        TryDrop();

        draggedObject = null;
        isPointerHeld = false;
    }

    Vector3 GetPointerWorldPos()
    {
        Vector3 screenPoint = Pointer.current.position.ReadValue();
        screenPoint.z = objectDepth;
        return cam.ScreenToWorldPoint(screenPoint);
    }

    void TryDrop()
    {
        Collider[] hits = Physics.OverlapSphere(draggedObject.position, snapDistance);

        foreach (Collider col in hits)
        {
            if (col.CompareTag(targetTag))
            {
                draggedObject.position = col.transform.position;
                draggedObject.rotation = col.transform.rotation;

                OnDroppedCorrectly?.Invoke();
                return;
            }
        }

        // Reset if wrong
        draggedObject.position = startPos;
        draggedObject.rotation = startRot;
    }
}
