using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class PopupQuizButton_SS : MonoBehaviour
{
    [Header("Main Button")]
    [SerializeField] private Button mainButton;
    [SerializeField] private TextMeshProUGUI mainButtonText;

    [Header("Popup Panel")]
    [SerializeField] private GameObject popupPanel;

    [Header("Answer Buttons")]
    [SerializeField] private Button correctButton;
    [SerializeField] private Button[] wrongButtons;

    [Header("Icons")]
    [SerializeField] private GameObject correctIcon;
    [SerializeField] private GameObject wrongIcon1;
    [SerializeField] private GameObject wrongIcon2;

    [Header("Delay")]
    [SerializeField] private float correctDelay = 1f;

    [Header("Blink Highlight Wings")]
    [SerializeField] private List<Renderer> blinkWings;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private float blinkSpeed = 2f;

    // 🔹 SESSION MEMORY (per button instance)
    private static Dictionary<int, string> sessionAnswers =
        new Dictionary<int, string>();

    private int instanceKey;

    private bool answered;
    private Coroutine correctRoutine;
    private List<Coroutine> blinkRoutines = new List<Coroutine>();
    private List<Material> originalMaterials = new List<Material>();

    // -------------------- LIFECYCLE --------------------

    private void Awake()
    {
        // Unique key per slide / button
        instanceKey = GetInstanceID();
    }

    private void OnEnable()
    {
        answered = false;

        popupPanel.SetActive(false);
        correctIcon.SetActive(false);
        wrongIcon1.SetActive(false);
        wrongIcon2.SetActive(false);

        BindButtons();

        // Store original materials for wings
        originalMaterials.Clear();
        foreach (Renderer rend in blinkWings)
        {
            if (rend != null)
                originalMaterials.Add(rend.material);
            else
                originalMaterials.Add(null);
        }

        // ✅ Restore ONLY this slide's answer
        if (sessionAnswers.ContainsKey(instanceKey))
        {
            mainButtonText.text = sessionAnswers[instanceKey];
            answered = true;
        }
        else
        {
            StartAllBlink();
        }
    }

    private void OnDisable()
    {
        StopAllBlink();

        if (correctRoutine != null)
            StopCoroutine(correctRoutine);

        UnbindButtons();
    }

    // -------------------- BUTTON BINDING --------------------

    private void BindButtons()
    {
        mainButton.onClick.RemoveAllListeners();
        mainButton.onClick.AddListener(OpenPopup);

        correctButton.onClick.RemoveAllListeners();
        correctButton.onClick.AddListener(OnCorrectClicked);

        for (int i = 0; i < wrongButtons.Length; i++)
        {
            int index = i;
            wrongButtons[i].onClick.RemoveAllListeners();
            wrongButtons[i].onClick.AddListener(() => OnWrongClicked(index));
        }
    }

    private void UnbindButtons()
    {
        mainButton.onClick.RemoveAllListeners();
        correctButton.onClick.RemoveAllListeners();

        foreach (Button btn in wrongButtons)
            btn.onClick.RemoveAllListeners();
    }

    // -------------------- MAIN BUTTON --------------------

    private void OpenPopup()
    {
        if (answered) return;

        popupPanel.SetActive(true);
        HideWrongIcons();
    }

    // -------------------- CORRECT --------------------

    private void OnCorrectClicked()
    {
        if (answered) return;

        answered = true;

        StopAllBlink();
        HideWrongIcons();
        correctIcon.SetActive(true);

        correctRoutine = StartCoroutine(CorrectDelayRoutine());
    }

    private IEnumerator CorrectDelayRoutine()
    {
        yield return new WaitForSeconds(correctDelay);

        string answerText =
            correctButton.GetComponentInChildren<TextMeshProUGUI>().text;

        mainButtonText.text = answerText;

        // ✅ Save only for THIS button
        sessionAnswers[instanceKey] = answerText;

        popupPanel.SetActive(false);
    }

    // -------------------- WRONG --------------------

    private void OnWrongClicked(int index)
    {
        if (answered) return;

        StopAllBlink();
        HideWrongIcons();

        if (index == 0)
            wrongIcon1.SetActive(true);
        else if (index == 1)
            wrongIcon2.SetActive(true);
    }

    private void HideWrongIcons()
    {
        wrongIcon1.SetActive(false);
        wrongIcon2.SetActive(false);
    }

    // =====================================================
    // Blink Highlight for Wings
    // =====================================================

    private void StartAllBlink()
    {
        StopAllBlink();

        blinkRoutines.Clear();
        for (int i = 0; i < blinkWings.Count; i++)
        {
            if (blinkWings[i] != null && highlightMaterial != null)
            {
                Coroutine routine = StartCoroutine(
                    BlinkRoutine(blinkWings[i], originalMaterials[i], highlightMaterial));
                blinkRoutines.Add(routine);
            }
        }
    }

    private void StopAllBlink()
    {
        foreach (Coroutine routine in blinkRoutines)
        {
            if (routine != null)
                StopCoroutine(routine);
        }
        blinkRoutines.Clear();

        RestoreOriginalMaterials();
    }

    private void RestoreOriginalMaterials()
    {
        for (int i = 0; i < blinkWings.Count; i++)
        {
            if (blinkWings[i] != null && originalMaterials[i] != null)
            {
                blinkWings[i].material = originalMaterials[i];
            }
        }
    }

    private IEnumerator BlinkRoutine(Renderer rend, Material original, Material highlight)
    {
        Material matInstance = new Material(original);
        rend.material = matInstance;

        while (true)
        {
            float t = (Mathf.Sin(Time.time / blinkSpeed * Mathf.PI * 2f) + 1f) / 2f;
            matInstance.Lerp(original, highlight, t);
            yield return null;
        }
    }
}
