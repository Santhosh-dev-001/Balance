using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class SlideFlowController : MonoBehaviour
{
    [System.Serializable]
    public class Slide
    {
        public GameObject slideUI;
        public Transform cameraPoint;

        public UnityEvent onEnter;
        public UnityEvent onExit;

        [Header("Unlock")]
        public bool unlockOnEnter;

        [Header("Optional Back Camera")]
        public bool useCustomBackPoint;
        public Transform customBackCameraPoint;
    }

    [Header("Slides")]
    public List<Slide> slides = new List<Slide>();

    [Header("Camera")]
    public Transform mainCamera;
    public float cameraSpeed = 2f;

    [Header("Navigation")]
    public Button nextButton;
    public Button backButton;

    [Header("UI")]
    public TextMeshProUGUI slideCounterText;

    [Header("Slide Counter Settings")]
    public int startingSlideNumber = 1;

    [Header("First Slide Back Custom Event")]
    public GameObject firstBackEnableObject;
    public GameObject firstBackDisableObject;

    [Header("Debug")]
    public bool enableCheatKey = false;

    int currentIndex = 0;
    bool isMoving = false;
    bool[] slideCompleted;
    Coroutine cameraRoutine;

    void Start()
    {
        slideCompleted = new bool[slides.Count];

        if (nextButton != null)
            nextButton.onClick.AddListener(NextSlide);

        if (backButton != null)
            backButton.onClick.AddListener(PreviousSlide);

        if (slides.Count > 0)
            ShowSlide(0);
    }

    void Update()
    {
        if (!enableCheatKey) return;

        if (Input.GetKeyDown(KeyCode.N))
        {
            UnlockNext();
        }
    }

    // ===================== CORE FLOW =====================

    void ShowSlide(int index)
    {
        if (index < 0 || index >= slides.Count) return;

        foreach (var s in slides)
        {
            if (s.slideUI != null)
                s.slideUI.SetActive(false);
        }

        if (currentIndex < slides.Count)
            slides[currentIndex].onExit?.Invoke();

        currentIndex = index;

        if (slides[currentIndex].slideUI != null)
            slides[currentIndex].slideUI.SetActive(true);

        slides[currentIndex].onEnter?.Invoke();

        if (slides[currentIndex].unlockOnEnter)
            UnlockNext();

        if (slides[currentIndex].cameraPoint != null)
            MoveCamera(slides[currentIndex].cameraPoint);

        UpdateUI();
    }

    public void NextSlide()
    {
        if (isMoving) return;
        if (!slideCompleted[currentIndex]) return;
        if (currentIndex >= slides.Count - 1) return;

        ShowSlide(currentIndex + 1);
    }

    void PreviousSlide()
    {
        if (isMoving) return;

        // 🔥 First slide custom back event
        if (currentIndex == 0)
        {
            ExecuteFirstSlideBackTrigger();
            return;
        }

        Slide previousSlide = slides[currentIndex - 1];

        if (slides[currentIndex].useCustomBackPoint &&
            slides[currentIndex].customBackCameraPoint != null)
        {
            MoveCamera(slides[currentIndex].customBackCameraPoint);
        }
        else if (previousSlide.cameraPoint != null)
        {
            MoveCamera(previousSlide.cameraPoint);
        }

        ShowSlide(currentIndex - 1);
    }

    // ===================== UNIQUE BACK METHOD =====================

    public void ExecuteFirstSlideBackTrigger()
    {
        if (firstBackEnableObject != null)
            firstBackEnableObject.SetActive(true);

        if (firstBackDisableObject != null)
            firstBackDisableObject.SetActive(false);
    }

    // ===================== CAMERA =====================

    void MoveCamera(Transform target)
    {
        if (target == null || mainCamera == null) return;

        if (cameraRoutine != null)
            StopCoroutine(cameraRoutine);

        cameraRoutine = StartCoroutine(CameraMove(target));
    }

    IEnumerator CameraMove(Transform target)
    {
        isMoving = true;

        Vector3 startPos = mainCamera.position;
        Quaternion startRot = mainCamera.rotation;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * cameraSpeed;
            mainCamera.position = Vector3.Lerp(startPos, target.position, t);
            mainCamera.rotation = Quaternion.Slerp(startRot, target.rotation, t);
            yield return null;
        }

        isMoving = false;
    }

    // ===================== UI =====================

    void UpdateUI()
    {
        if (slideCounterText != null)
        {
            int displayNumber = startingSlideNumber + currentIndex;
            slideCounterText.text = displayNumber + " / " + (startingSlideNumber + slides.Count - 1);
        }

        // ✅ Back button always interactable
        if (backButton != null)
            backButton.interactable = true;

        if (nextButton != null)
            nextButton.interactable = slideCompleted[currentIndex];
    }

    // ===================== CALLED FROM INTERACTIONS =====================

    public void UnlockNext()
    {
        if (currentIndex < 0 || currentIndex >= slideCompleted.Length) return;

        slideCompleted[currentIndex] = true;
        UpdateUI();
    }
}
