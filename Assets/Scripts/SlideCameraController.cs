using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class SlideCameraController : MonoBehaviour
{
    [System.Serializable]
    public class Step
    {
        public Transform cameraPoint;
        public GameObject slideUI;

        public UnityEvent onNextClicked;
        public UnityEvent onBackClicked;
        public UnityEvent onRepeatedNextClicked;

        [Header("Special UI Controls")]
        public bool enableNextOnStart = false;

        public bool showNumpad = false;
        public GameObject numpadObject;

        public bool showCalculator = false;
        public GameObject calculatorObject;
    }

    [Header("Camera")]
    public Transform cameraTransform;
    public float moveSpeed = 2f;

    [Header("Steps")]
    public List<Step> steps;

    public Button nextButton;
    public Button previousButton;

    [Header("Slide Counter UI")]
    public TextMeshProUGUI slideCounterText;

    [Header("Slide Counter Values")]
    public int currentSlide;
    public int totalSlides;

    [Header("Debug / Cheat")]
    public bool enableCheatKey = true;

    int currentIndex = 0;
    bool isMoving = false;

    bool[] slideCompleted;
    bool[] slideLocked;
    bool[] slideVisited;
    bool[] eventUsed;

    void Start()
    {
        previousButton.interactable = true;

        slideCompleted = new bool[steps.Count];
        slideLocked = new bool[steps.Count];
        slideVisited = new bool[steps.Count];
        eventUsed = new bool[steps.Count];

        UpdateSlideCounterUI();

        foreach (var s in steps)
            s.slideUI.SetActive(false);

        steps[0].slideUI.SetActive(true);

        cameraTransform.position = steps[0].cameraPoint.position;
        cameraTransform.rotation = steps[0].cameraPoint.rotation;

        nextButton.interactable = false;

        ApplyStepSettings(0);
        nextButton.interactable = slideCompleted[0];

        UpdateBackButton();
    }

    void Update()
    {
        if (enableCheatKey && Input.GetKeyDown(KeyCode.N))
        {
            ForceNextSlide_Cheat();
        }

#if ENABLE_INPUT_SYSTEM
        if (enableCheatKey &&
            UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.nKey.wasPressedThisFrame)
        {
            ForceNextSlide_Cheat();
        }
#endif
    }

    void ForceNextSlide_Cheat()
    {
        if (currentIndex >= steps.Count - 1)
            return;

        StopAllCoroutines();
        isMoving = false;

        slideCompleted[currentIndex] = true;
        slideLocked[currentIndex] = true;

        nextButton.interactable = true;
        UpdateBackButton();

        StartCoroutine(MoveTo(currentIndex + 1));
    }

    public void EnableNextButton()
    {
        slideCompleted[currentIndex] = true;
        slideLocked[currentIndex] = true;
        nextButton.interactable = true;

        UpdateBackButton();
    }

    public void Next()
    {
        if (isMoving) return;
        if (!slideCompleted[currentIndex]) return;

        if (!eventUsed[currentIndex])
        {
            steps[currentIndex].onNextClicked?.Invoke();
            eventUsed[currentIndex] = true;
        }

        steps[currentIndex].onRepeatedNextClicked?.Invoke();

        if (currentIndex >= steps.Count - 1)
            return;

        StartCoroutine(MoveTo(currentIndex + 1));
    }

    public void Previous()
    {
        StopAllCoroutines();
        isMoving = false;

        steps[currentIndex].onBackClicked?.Invoke();

        if (currentIndex > 0)
        {
            StartCoroutine(MoveTo(currentIndex - 1));
        }
    }

    IEnumerator MoveTo(int targetIndex)
    {
        isMoving = true;

        foreach (var s in steps)
            s.slideUI.SetActive(false);

        Vector3 startPos = cameraTransform.position;
        Quaternion startRot = cameraTransform.rotation;

        Vector3 endPos = steps[targetIndex].cameraPoint.position;
        Quaternion endRot = steps[targetIndex].cameraPoint.rotation;

        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * moveSpeed;
            cameraTransform.position = Vector3.Lerp(startPos, endPos, t);
            cameraTransform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        int previousIndex = currentIndex;
        currentIndex = targetIndex;

        // 🔥 ONLY ADDED THIS SECTION
        if (currentIndex > previousIndex)
            currentSlide++;
        else if (currentIndex < previousIndex)
            currentSlide--;

        UpdateSlideCounterUI();

        steps[currentIndex].slideUI.SetActive(true);

        ApplyStepSettings(currentIndex);

        nextButton.interactable = slideCompleted[currentIndex];
        UpdateBackButton();

        isMoving = false;
    }

    void ApplyStepSettings(int index)
    {
        Step s = steps[index];

        if (slideLocked[index])
        {
            slideCompleted[index] = true;
            nextButton.interactable = true;

            if (s.numpadObject != null)
                s.numpadObject.SetActive(false);

            if (s.calculatorObject != null)
                s.calculatorObject.SetActive(false);

            return;
        }

        slideVisited[index] = true;

        if (s.enableNextOnStart)
        {
            slideCompleted[index] = true;
        }

        if (s.numpadObject != null)
            s.numpadObject.SetActive(s.showNumpad);

        if (s.calculatorObject != null)
            s.calculatorObject.SetActive(s.showCalculator);
    }

    public void EnableNextSlideNextButton()
    {
        int nextIndex = currentIndex + 1;
        if (nextIndex < steps.Count)
        {
            slideCompleted[nextIndex] = true;
        }
    }

    void UpdateBackButton()
    {
        if (currentIndex == 0)
        {
            previousButton.interactable = true;
            return;
        }

        previousButton.interactable = slideCompleted[currentIndex];
    }

    void UpdateSlideCounterUI()
    {
        if (slideCounterText != null)
        {
            slideCounterText.text = (currentSlide + 1) + " / " + totalSlides;
        }
    }
}
