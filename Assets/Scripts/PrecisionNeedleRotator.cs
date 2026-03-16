using UnityEngine;
using UnityEngine.EventSystems;

public class PrecisionNeedleRotator : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Ring Object (Drag This)")]
    public Transform ringObject;

    [Header("Down Needle (Smooth Rotate)")]
    public Transform downNeedle;

    [Header("Correct UI")]
    public GameObject correctUI;

    [Header("Slide Flow Controller")]
    public SlideFlowController slideFlowController;  // 🔥 reference here

    [Header("Axis Control")]
    public bool moveX = true;
    public bool moveY = false;
    public bool moveZ = false;

    [Header("Snap Settings")]
    public float snapStep = 0.02f;
    public float minLimit = -2f;
    public float maxLimit = 2f;

    [Header("Needle Smooth Settings")]
    public float needleSmoothSpeed = 8f;
    public float needleRotationMultiplier = 50f;

    private Camera cam;
    private float objectScreenZ;
    private Vector3 dragOffset;
    private float targetNeedleAngle;

    private bool isLocked = false;

    void Start()
    {
        cam = Camera.main;

        if (ringObject == null)
            ringObject = transform;

        if (correctUI != null)
            correctUI.SetActive(false);
    }

    void Update()
    {
        SmoothNeedleRotate();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isLocked) return;

        objectScreenZ = cam.WorldToScreenPoint(ringObject.position).z;

        Vector3 screenPoint = new Vector3(
            eventData.position.x,
            eventData.position.y,
            objectScreenZ);

        Vector3 worldPoint = cam.ScreenToWorldPoint(screenPoint);
        dragOffset = ringObject.position - worldPoint;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isLocked) return;

        Vector3 screenPoint = new Vector3(
            eventData.position.x,
            eventData.position.y,
            objectScreenZ);

        Vector3 worldPoint = cam.ScreenToWorldPoint(screenPoint);
        Vector3 targetPos = worldPoint + dragOffset;

        Vector3 finalPos = ringObject.position;

        if (moveX)
        {
            float value = Mathf.Clamp(targetPos.x, minLimit, maxLimit);
            finalPos.x = Mathf.Round(value / snapStep) * snapStep;
            targetNeedleAngle = finalPos.x * needleRotationMultiplier;
        }

        if (moveY)
        {
            float value = Mathf.Clamp(targetPos.y, minLimit, maxLimit);
            finalPos.y = Mathf.Round(value / snapStep) * snapStep;
            targetNeedleAngle = finalPos.y * needleRotationMultiplier;
        }

        if (moveZ)
        {
            float value = Mathf.Clamp(targetPos.z, minLimit, maxLimit);
            finalPos.z = Mathf.Round(value / snapStep) * snapStep;
            targetNeedleAngle = finalPos.z * needleRotationMultiplier;
        }

        ringObject.position = finalPos;
    }

    public void OnEndDrag(PointerEventData eventData) { }

    void SmoothNeedleRotate()
    {
        if (downNeedle == null) return;

        Quaternion targetRot = Quaternion.Euler(0f, 0f, -targetNeedleAngle);

        downNeedle.localRotation = Quaternion.Lerp(
            downNeedle.localRotation,
            targetRot,
            Time.deltaTime * needleSmoothSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CenterTarget") && !isLocked)
        {
            isLocked = true;  // 🔥 stop drag permanently

            if (correctUI != null)
                correctUI.SetActive(true);

            // 🔥 Proper way according to your SlideFlowController
            if (slideFlowController != null)
                slideFlowController.UnlockNext();
        }
    }
}
