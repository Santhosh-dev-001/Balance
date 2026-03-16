using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class DialerSUI_SSS: MonoBehaviour
{
    [Header("Input Field")]
    public TMP_InputField valueField;

    [Header("Expected Value")]
    public float expectedValue;

    [Header("Correct Symbol")]
    public GameObject correctSymbol;

    [Header("Check Button")]
    public Button checkButton;

    [Header("Message UI")]
    public TextMeshProUGUI messageText;

    [Header("Message Settings")]
    public float messageHideDelay = 3f;

    const float TOL = 0.001f;

    bool retryMode = false;
    Coroutine messageRoutine;

    void Start()
    {
        valueField.interactable = true;
        valueField.text = "";

        correctSymbol?.SetActive(false);

        if (messageText)
        {
            messageText.text = "";
            messageText.gameObject.SetActive(false);
        }

        checkButton.onClick.AddListener(OnCheckPress);
    }

    // ================== CHECK LOGIC ==================

    public void OnCheckPress()
    {
        // Empty check
        if (string.IsNullOrEmpty(valueField.text))
        {
            ShowMessage("Please enter a value");
            return;
        }

        HideMessage();

        // Retry clears field
        if (retryMode)
        {
            valueField.text = "";
            retryMode = false;
            return;
        }

        if (!float.TryParse(valueField.text, out float val))
            return;

        if (Mathf.Abs(val - expectedValue) < TOL)
        {
            correctSymbol?.SetActive(true);
            valueField.interactable = false;
            retryMode = false;
        }
        else
        {
            retryMode = true;
        }
    }

    // ================== MESSAGE HELPERS ==================

    void ShowMessage(string msg)
    {
        if (!messageText) return;

        messageText.text = msg;
        messageText.gameObject.SetActive(true);

        if (messageRoutine != null)
            StopCoroutine(messageRoutine);

        messageRoutine = StartCoroutine(HideMessageAfterDelay());
    }

    IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageHideDelay);
        HideMessage();
    }

    void HideMessage()
    {
        if (messageRoutine != null)
        {
            StopCoroutine(messageRoutine);
            messageRoutine = null;
        }

        if (messageText)
            messageText.gameObject.SetActive(false);
    }
}
