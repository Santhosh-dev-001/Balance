using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialerUI_S : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_InputField minField;
    public TMP_InputField maxField;
    public TMP_InputField lcField;

    [Header("Expected Values")]
    public float expectedMinValue;
    public float expectedMaxValue;
    public float expectedLCValue;

    [Header("Correct Symbols")]
    public GameObject minCorrectSymbol;
    public GameObject maxCorrectSymbol;
    public GameObject lcCorrectSymbol;

    [Header("LC Question")]
    public GameObject lcPanel;

    [Header("Buttons")]
    public Button checkButton;
    public Button nextButton;
    public Button autoFillButton;

    [Header("Slide Controller")]
    public SlideFlowController pageNavigation;

    const float TOL = 0.01f;

    int stage = 0; // 0=min, 1=max, 2=lc, 3=done
    int wrongCount = 0;

    void Start()
    {
        ResetUI();

        checkButton.onClick.AddListener(OnCheck);
        nextButton.onClick.AddListener(OnNext);
        autoFillButton.onClick.AddListener(OnAutoFill);
    }

    void ResetUI()
    {
        minField.text = "";
        maxField.text = "";
        lcField.text = "";

        minField.interactable = true;
        maxField.interactable = false;
        lcField.interactable = false;

        minCorrectSymbol.SetActive(false);
        maxCorrectSymbol.SetActive(false);
        lcCorrectSymbol.SetActive(false);

        lcPanel.SetActive(false);

        nextButton.interactable = false;
        autoFillButton.gameObject.SetActive(false);

        stage = 0;
        wrongCount = 0;
    }

    TMP_InputField GetCurrentField()
    {
        if (stage == 0) return minField;
        if (stage == 1) return maxField;
        if (stage == 2) return lcField;
        return null;
    }

    float GetExpected()
    {
        if (stage == 0) return expectedMinValue;
        if (stage == 1) return expectedMaxValue;
        return expectedLCValue;
    }

    void OnCheck()
    {
        TMP_InputField field = GetCurrentField();
        if (field == null) return;

        float expected = GetExpected();

        if (!float.TryParse(field.text, out float value))
        {
            RegisterWrong(field);
            return;
        }

        if (Mathf.Abs(value - expected) >= TOL)
        {
            RegisterWrong(field);
            return;
        }

        // ✅ Correct
        wrongCount = 0;
        autoFillButton.gameObject.SetActive(false);

        if (stage == 0)
        {
            minCorrectSymbol.SetActive(true);
            minField.interactable = false;

            stage = 1;
            maxField.interactable = true;
        }
        else if (stage == 1)
        {
            maxCorrectSymbol.SetActive(true);
            maxField.interactable = false;

            // Directly show LC panel (No need Next button)
            lcPanel.SetActive(true);
            lcField.interactable = true;

            stage = 2;
        }
        else if (stage == 2)
        {
            lcCorrectSymbol.SetActive(true);
            lcField.interactable = false;

            stage = 3;

            // ✅ Auto Unlock Slide (No Next Button Needed)
            if (pageNavigation != null)
                pageNavigation.UnlockNext();
        }
    }

    void RegisterWrong(TMP_InputField field)
    {
        wrongCount++;
        field.text = "";

        if (wrongCount >= 3)
            autoFillButton.gameObject.SetActive(true);
    }

    void OnAutoFill()
    {
        wrongCount = 0;
        autoFillButton.gameObject.SetActive(false);

        if (stage == 0)
        {
            minField.text = expectedMinValue.ToString("0.##");
            minCorrectSymbol.SetActive(true);
            minField.interactable = false;

            stage = 1;
            maxField.interactable = true;
        }
        else if (stage == 1)
        {
            maxField.text = expectedMaxValue.ToString("0.##");
            maxCorrectSymbol.SetActive(true);
            maxField.interactable = false;

            lcPanel.SetActive(true);
            lcField.interactable = true;

            stage = 2;
        }
        else if (stage == 2)
        {
            lcField.text = expectedLCValue.ToString("0.##");
            lcCorrectSymbol.SetActive(true);
            lcField.interactable = false;

            stage = 3;

            // ✅ Auto Unlock Here Also
            if (pageNavigation != null)
                pageNavigation.UnlockNext();
        }
    }

    void OnNext()
    {
        // Not required anymore
    }

    public void OnNumberPress(string number)
    {
        TMP_InputField field = GetCurrentField();
        if (field != null && field.interactable)
            field.text += number;
    }

    public void OnClear()
    {
        TMP_InputField field = GetCurrentField();
        if (field != null && field.interactable)
            field.text = "";
    }
}
