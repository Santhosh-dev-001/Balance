using UnityEngine;
using UnityEngine.UI;

public class MCQController : MonoBehaviour
{
    [Header("Option Buttons")]
    public Button correctButton;
    public Button[] wrongButtons; // Size = 3

    [Header("Sprites")]
    public Sprite correctSprite;
    public Sprite wrongSprite;

    [Header("Why Buttons")]
    public Button whyWrongButton;
    public Button whyCorrectButton;

    [Header("Panels")]
    public GameObject wrongPanel;
    public GameObject correctPanel;

    // STATE
    private bool correctSelected = false;
    private bool panelOpen = false;

    private Image correctImage;
    private Image[] wrongImages;

    void Start()
    {
        correctImage = correctButton.GetComponent<Image>();

        wrongImages = new Image[wrongButtons.Length];
        for (int i = 0; i < wrongButtons.Length; i++)
        {
            int index = i;
            wrongImages[i] = wrongButtons[i].GetComponent<Image>();
            wrongButtons[i].onClick.AddListener(() => OnWrongClicked(index));
        }

        correctButton.onClick.AddListener(OnCorrectClicked);

        RestoreState();
    }

    void OnWrongClicked(int index)
    {
        if (correctSelected || panelOpen) return;

        // Mark this wrong button
        wrongImages[index].sprite = wrongSprite;

        whyWrongButton.gameObject.SetActive(true);
        whyCorrectButton.gameObject.SetActive(false);
    }

    void OnCorrectClicked()
    {
        if (correctSelected || panelOpen) return;

        correctSelected = true;

        correctImage.sprite = correctSprite;

        whyCorrectButton.gameObject.SetActive(true);
        whyWrongButton.gameObject.SetActive(false);
    }

    // ---------- WHY PANELS ----------

    public void OpenWrongPanel()
    {
        panelOpen = true;
        wrongPanel.SetActive(true);
        whyWrongButton.gameObject.SetActive(false);
    }

    public void CloseWrongPanel()
    {
        panelOpen = false;
        wrongPanel.SetActive(false);

        if (!correctSelected)
            whyWrongButton.gameObject.SetActive(true);
    }

    public void OpenCorrectPanel()
    {
        panelOpen = true;
        correctPanel.SetActive(true);
        whyCorrectButton.gameObject.SetActive(false);
    }

    public void CloseCorrectPanel()
    {
        panelOpen = false;
        correctPanel.SetActive(false);
        whyCorrectButton.gameObject.SetActive(true);
    }

    // ---------- RESTORE STATE ----------

    void RestoreState()
    {
        if (correctSelected)
        {
            correctImage.sprite = correctSprite;
            whyCorrectButton.gameObject.SetActive(true);
        }
    }
}
