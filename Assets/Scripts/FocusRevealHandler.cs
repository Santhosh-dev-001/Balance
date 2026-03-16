using UnityEngine;

public class FocusRevealHandler : MonoBehaviour
{
    [Header("Visual Setup")]
    public Material defaultSurface;
    public Material glowingSurface;

    [Header("Camera Focus Setup")]
    public Transform focusDestination;
    public float focusMoveSpeed = 3f;

    [Header("Animation")]
    public Animator objectAnimator;   // 👈 assign in inspector
    public string animationTriggerName = "Play"; // 👈 trigger name

    private Renderer meshRendererRef;
    private Transform sceneCamera;

    private bool shouldMoveFocus = false;
    private bool glowActive = true;

    private void Awake()
    {
        meshRendererRef = GetComponent<Renderer>();

        // Automatically get Main Camera
        if (Camera.main != null)
        {
            sceneCamera = Camera.main.transform;
        }

        // Auto get animator if not assigned
        if (objectAnimator == null)
        {
            objectAnimator = GetComponent<Animator>();
        }

        // Scene start highlight ON
        if (meshRendererRef != null && glowingSurface != null)
        {
            meshRendererRef.material = glowingSurface;
            glowActive = true;
        }
    }

    private void Update()
    {
        DetectUserInteraction();
        ProcessCameraFocusMovement();
    }

    private void DetectUserInteraction()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray clickRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            HandleRaycast(clickRay);
        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Ray touchRay = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            HandleRaycast(touchRay);
        }
    }

    private void HandleRaycast(Ray rayInput)
    {
        if (Physics.Raycast(rayInput, out RaycastHit hitInfo))
        {
            if (hitInfo.transform == transform)
            {
                TriggerFocusSequence();
            }
        }
    }

    private void TriggerFocusSequence()
    {
        if (!glowActive) return;

        // Highlight OFF
        if (meshRendererRef != null && defaultSurface != null)
        {
            meshRendererRef.material = defaultSurface;
            glowActive = false;
        }

        // ✅ PLAY ANIMATION
        if (objectAnimator != null)
        {
            objectAnimator.SetTrigger(animationTriggerName);
        }

        // Start camera move
        shouldMoveFocus = true;
    }

    private void ProcessCameraFocusMovement()
    {
        if (!shouldMoveFocus || sceneCamera == null || focusDestination == null)
            return;

        sceneCamera.position = Vector3.Lerp(
            sceneCamera.position,
            focusDestination.position,
            focusMoveSpeed * Time.deltaTime
        );

        sceneCamera.rotation = Quaternion.Slerp(
            sceneCamera.rotation,
            focusDestination.rotation,
            focusMoveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(sceneCamera.position, focusDestination.position) < 0.02f)
        {
            shouldMoveFocus = false;
        }
    }
}
