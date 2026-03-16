using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialerUI_LC : MonoBehaviour
{
    public TMP_InputField lcField;
    public float expectedLCValue;
    public GameObject lcCorrectSymbol;
    public Button checkButton;

    const float TOL = 0.001f;

    void Start()
    {
        lcField.text = "";
        lcField.interactable = true;

        lcCorrectSymbol?.SetActive(false);

        checkButton.onClick.RemoveAllListeners();
        checkButton.onClick.AddListener(OnCheck);
    }

    // ================= KEYPAD =================

    public void OnNumberPress(string number)
    {
        lcField.text += number;
    }

    public void OnDecimalPress()
    {
        if (!lcField.text.Contains("."))
            lcField.text = lcField.text == "" ? "0." : lcField.text + ".";
    }

    public void OnBackspacePress()
    {
        if (lcField.text.Length > 0)
            lcField.text =
                lcField.text.Substring(0, lcField.text.Length - 1);
    }

    public void OnClearPress()
    {
        lcField.text = "";
    }

    // ================= CHECK =================

    void OnCheck()
    {
        if (!float.TryParse(lcField.text, out float val)) return;

        if (Mathf.Abs(val - expectedLCValue) < TOL)
        {
            lcCorrectSymbol?.SetActive(true);
            lcField.interactable = false;
        }
        else
        {
            lcField.text = "";
        }
    }
}
