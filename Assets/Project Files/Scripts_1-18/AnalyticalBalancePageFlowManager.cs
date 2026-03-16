using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class AnalyticalBalancePageFlowManager : MonoBehaviour
{
    [Header("Experiment Steps")]
    [SerializeField] private List<ExperimentStepPage> stepPages = new List<ExperimentStepPage>();

    [Header("Navigation Buttons")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;

    [Header("Camera Settings")]
    [SerializeField] private Transform mainCamera;
    [SerializeField] private float defaultMoveSpeed = 4f;

    [Header("UI")]
    [SerializeField] private TMP_Text stepCounterText;
    [SerializeField] private int pageOffset = 1;

    // ✅ ADDED
    [Header("Page Number Settings")]
    [SerializeField] private int startingPageNumber = 1;
    [SerializeField] private int totalSlides = 0;

    [Header("Final Step Event")]
    [SerializeField] private UnityEvent OnExperimentFinished;

    [SerializeField] private BalanceComponentFocusSystem componentFocusSystem;

    private int currentStepIndex = 0;
    private bool navigationLocked = false;
    private bool isNextUnlocked = false;
    private bool[] stepCompleted;
    private Coroutine camMoveRoutine;

    [System.Serializable]
    public class ExperimentStepPage
    {
        [SerializeField] private GameObject pageRoot;
        [SerializeField] private Transform cameraFocusPoint;
        [SerializeField] private float cameraMoveSpeed = 4f;
        [SerializeField] private UnityEvent onStepEnter;
        [SerializeField] private UnityEvent onStepExit;
        [SerializeField] private UnityEvent onBackEvent;

        public GameObject PageRoot => pageRoot;
        public Transform CameraFocusPoint => cameraFocusPoint;
        public float CameraMoveSpeed => cameraMoveSpeed;
        public UnityEvent OnStepEnter => onStepEnter;
        public UnityEvent OnStepExit => onStepExit;
        public UnityEvent OnBackEvent => onBackEvent;
    }

    private void Start()
    {
        if (stepPages.Count == 0)
        {
            Debug.LogError("No Analytical Balance Steps Assigned!");
            return;
        }

        stepCompleted = new bool[stepPages.Count];
        for (int i = 0; i < stepCompleted.Length; i++)
            stepCompleted[i] = false;

        DisplayStep(0);

        nextButton.onClick.AddListener(GoToNextStep);
        previousButton.onClick.AddListener(GoToPreviousStep);

        RefreshUI();
    }

    private void GoToNextStep()
    {
        if (!isNextUnlocked || navigationLocked) return;

        if (currentStepIndex < stepPages.Count - 1)
            SwitchStep(currentStepIndex + 1);
        else
        {
            Debug.Log("Experiment Completed ??");
            OnExperimentFinished?.Invoke();
        }
    }

    private void GoToPreviousStep()
    {
        if (currentStepIndex <= 0 || navigationLocked) return;

        stepPages[currentStepIndex].OnBackEvent?.Invoke();
        SwitchStep(currentStepIndex - 1);
    }

    private void SwitchStep(int newIndex)
    {
        stepPages[currentStepIndex].OnStepExit?.Invoke();
        currentStepIndex = newIndex;
        DisplayStep(currentStepIndex);
    }

    private void DisplayStep(int index)
    {
        for (int i = 0; i < stepPages.Count; i++)
            if (stepPages[i].PageRoot)
                stepPages[i].PageRoot.SetActive(i == index);

        stepPages[index].OnStepEnter?.Invoke();
        MoveCamera(stepPages[index]);

        isNextUnlocked = stepCompleted[index];
        RefreshUI();
    }

    private void MoveCamera(ExperimentStepPage step)
    {
        if (!step.CameraFocusPoint || !mainCamera) return;

        if (camMoveRoutine != null)
            StopCoroutine(camMoveRoutine);

        float speed = step.CameraMoveSpeed > 0 ? step.CameraMoveSpeed : defaultMoveSpeed;
        camMoveRoutine = StartCoroutine(SmoothMove(step.CameraFocusPoint, speed));
    }

    private IEnumerator SmoothMove(Transform target, float speed)
    {
        while (Vector3.Distance(mainCamera.position, target.position) > 0.02f)
        {
            mainCamera.position = Vector3.Lerp(mainCamera.position, target.position, Time.deltaTime * speed);
            mainCamera.rotation = Quaternion.Slerp(mainCamera.rotation, target.rotation, Time.deltaTime * speed);
            yield return null;
        }
    }

    private void RefreshUI()
    {
        if (stepCounterText)
        {
            int displayedPageNumber = startingPageNumber + currentStepIndex;
            int displayedTotal = totalSlides > 0 ? totalSlides : stepPages.Count;

            stepCounterText.text = $"{displayedPageNumber} / {displayedTotal}";
        }

        previousButton.interactable = currentStepIndex > 0 && !navigationLocked;
        nextButton.interactable = isNextUnlocked && !navigationLocked;
    }

    // Call this when a step action is completed
    public void UnlockNextStep()
    {
        isNextUnlocked = true;
        stepCompleted[currentStepIndex] = true;
        RefreshUI();
    }
}
