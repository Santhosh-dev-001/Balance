using UnityEngine;
using System.Collections;

public class ObjectClickController : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material highlightMaterial;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string animationTriggerName = "Play";
    [SerializeField] private float animationSpeed = 1.5f;

    [Header("Camera Settings")]
    [SerializeField] private Transform mainCamera;
    [SerializeField] private Transform cameraTargetPoint;
    [SerializeField] private float cameraSmoothSpeed = 3.5f;

    [Header("Object Final Move")]
    [SerializeField] private Transform objectFinalPosition;
    [SerializeField] private float objectMoveSpeed = 2f;

    [Header("Slide Flow Controller")]
    [SerializeField] private SlideFlowController slideFlowController; // 🔥 assign this in inspector

    private Renderer objectRenderer;

    private bool moveCamera = false;
    private bool moveObjectFinal = false;

    private void Start()
    {
        objectRenderer = GetComponent<Renderer>();

        if (objectRenderer != null && normalMaterial != null)
            objectRenderer.material = normalMaterial;
    }

    private void Update()
    {
        HandleInput();
        MoveCamera();
        MoveObjectFinal();
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            CheckHit(ray);
        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            CheckHit(ray);
        }
    }

    private void CheckHit(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform == transform)
            {
                OnObjectClicked();
            }
        }
    }

    private void OnObjectClicked()
    {
        // Change material
        if (objectRenderer != null && highlightMaterial != null)
            objectRenderer.material = highlightMaterial;

        // 🔥 ENABLE NEXT BUTTON IMMEDIATELY
        if (slideFlowController != null)
            slideFlowController.UnlockNext();

        // Continue normal flow
        StartCoroutine(PlayAnimationThenMoveCamera());
    }

    private IEnumerator PlayAnimationThenMoveCamera()
    {
        if (animator != null)
        {
            animator.speed = animationSpeed;
            animator.SetTrigger(animationTriggerName);

            yield return null;

            AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);

            if (clipInfo.Length > 0)
            {
                float clipLength = clipInfo[0].clip.length;
                yield return new WaitForSeconds(clipLength / animationSpeed);
            }
        }

        moveCamera = true;
    }

    private void MoveCamera()
    {
        if (!moveCamera || mainCamera == null || cameraTargetPoint == null)
            return;

        mainCamera.position = Vector3.Lerp(
            mainCamera.position,
            cameraTargetPoint.position,
            cameraSmoothSpeed * Time.deltaTime
        );

        mainCamera.rotation = Quaternion.Slerp(
            mainCamera.rotation,
            cameraTargetPoint.rotation,
            cameraSmoothSpeed * Time.deltaTime
        );

        if (Vector3.Distance(mainCamera.position, cameraTargetPoint.position) < 0.02f)
        {
            moveCamera = false;
            moveObjectFinal = true;
        }
    }

    private void MoveObjectFinal()
    {
        if (!moveObjectFinal || objectFinalPosition == null)
            return;

        transform.position = Vector3.Lerp(
            transform.position,
            objectFinalPosition.position,
            objectMoveSpeed * Time.deltaTime
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            objectFinalPosition.rotation,
            objectMoveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, objectFinalPosition.position) < 0.02f)
        {
            moveObjectFinal = false;
        }
    }
}
