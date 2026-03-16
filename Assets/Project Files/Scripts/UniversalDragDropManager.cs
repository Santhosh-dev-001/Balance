using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Events;
using Unity.VisualScripting;

public class UniversalDragDropManager : MonoBehaviour
{
    [System.Serializable]
    public class DraggableItem
    {
        public GameObject draggableObject;
        [Header("Weight Icon")]
public GameObject weightIcon;
 
        [Header("Weight Settings")]
        public float weightValue = 1f;
        public bool weightIsInMilligrams = false;

        [Header("Slot Visual Object")]
        public GameObject objectToEnableOnCorrectDrop;

        [HideInInspector] public Vector3 startPosition;
        [HideInInspector] public bool isPlaced;
        [HideInInspector] public Transform occupiedSlot;

        public float GetWeightInGrams()
        {
            return weightIsInMilligrams ? weightValue / 1000f : weightValue;
        }
    }

    [System.Serializable]
    public class SlidePuzzleData
    {
        public float requiredWeight = 0f;
        public bool weightIsInMilligrams = false;
[Header("Slide Animation")]
public string animationName;

        public GameObject displayPanel;
        public TMP_Text totalWeightText;
        public Image resultImage;


        [HideInInspector] public float currentTotalWeight = 0f;
        [HideInInspector] public Dictionary<Transform, DraggableItem> savedSlots =
            new Dictionary<Transform, DraggableItem>();

        [HideInInspector] public bool isSolved = false;
    }

    [Header("Draggable Items")]
    public List<DraggableItem> items = new List<DraggableItem>();

    [Header("Slides")]
    public List<SlidePuzzleData> slidesData = new List<SlidePuzzleData>();
private bool playAnimationNextTime = false;
    [Header("Weight Slots")]
    public List<Transform> weightSlots = new List<Transform>();
    private Dictionary<Transform, DraggableItem> occupiedSlots =
        new Dictionary<Transform, DraggableItem>();

    [Header("Drop Zone")]
    public Collider dropZone;
[Header("Common Animator")]
public Animator slideAnimator;

    [Header("Validation Panel")]
    public GameObject validationPanel;
    public TMP_Text validationText;

    [Header("Result Sprites")]
    public Sprite correctSprite;
    public Sprite wrongSprite;
[Header("External Trigger")]
public bool isClicked = false;

    public GameObject setResetButton;
    public TMP_Text buttonText;

    private SlidePuzzleData currentSlide;
    private int currentSlideIndex = 0;
 public UnityEvent oncorrectWeight
 ;
    private Camera cam;
    private DraggableItem currentItem;
    private bool isResetMode = false;
    public GameObject weight_gauge;

    // =====================================================
    // START
    // =====================================================

    void Start()
    {
        cam = Camera.main;

        if (setResetButton != null)
            setResetButton.SetActive(false);
            //setResetButton. gameObject.GetComponent<Button>.onClick.AddListener(setResetButton);


        foreach (var item in items)
        {
            item.startPosition = item.draggableObject.transform.position;
            if (item.weightIcon != null)
    item.weightIcon.SetActive(false);

            if (item.objectToEnableOnCorrectDrop != null)
                item.objectToEnableOnCorrectDrop.SetActive(false);
        }

        LoadSlide(0);
    }

    void Update()
    {
        HandleDrag();
    }
    public void istrue()
    {
        isClicked=true; 
    }

    // =====================================================
    // DRAG SYSTEM
    // =====================================================

    void HandleDrag()
    {
        if (currentSlide.isSolved) return; // 🔥 lock dragging if solved

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                foreach (var item in items)
                {
                    if (hit.collider.gameObject == item.draggableObject && !item.isPlaced)
                    {
                        currentItem = item;
                        

if (currentItem.weightIcon != null)
    currentItem.weightIcon.SetActive(true);

                        break;
                    }
                }
            }
        }

        if (Input.GetMouseButton(0) && currentItem != null)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            Plane dragPlane = new Plane(
                -cam.transform.forward,
                currentItem.draggableObject.transform.position
            );

            if (dragPlane.Raycast(ray, out float distance))
                currentItem.draggableObject.transform.position =
                    ray.GetPoint(distance);
        }
if (Input.GetMouseButtonUp(0) && currentItem != null)
{
    TryPlaceInSlot(currentItem);

    if (currentItem.weightIcon != null)
        currentItem.weightIcon.SetActive(false);

    currentItem = null;
}

    }

    void TryPlaceInSlot(DraggableItem item)
    {
        if (!dropZone.bounds.Contains(item.draggableObject.transform.position))
        {
            item.draggableObject.transform.position = item.startPosition;
            ShowValidation("Drop weight on right pan.");
            item.weightIcon.SetActive(false);
            return;
        }

        Transform freeSlot = GetFreeSlot();
        if (freeSlot == null)
        {
            ShowValidation("Weight gauge is full.");
            item.draggableObject.transform.position = item.startPosition;
            return;
        }

        PlaceItem(item, freeSlot);
    }

    Transform GetFreeSlot()
    {
        foreach (var slot in weightSlots)
            if (!occupiedSlots.ContainsKey(slot))
                return slot;
        return null;
    }

 void PlaceItem(DraggableItem item, Transform slot)
{
    if (item.weightIcon != null)
        item.weightIcon.SetActive(false);

    occupiedSlots[slot] = item;
    item.occupiedSlot = slot;
    item.isPlaced = true;

    item.draggableObject.SetActive(false);

    if (item.objectToEnableOnCorrectDrop != null)
    {
        GameObject obj = item.objectToEnableOnCorrectDrop;
        obj.SetActive(true);

        Renderer rend = obj.GetComponentInChildren<Renderer>();
        BoxCollider slotCollider = slot.GetComponent<BoxCollider>();

        if (rend != null && slotCollider != null)
        {
            // Get top of slot collider
            float slotTop = slotCollider.bounds.max.y;

            // Get bottom of object
            float objectBottom = rend.bounds.min.y;

            // Calculate offset
            float offset = slotTop - objectBottom;

            Vector3 newPos = obj.transform.position;

            // Snap horizontally
            newPos.x = slot.position.x;
            newPos.z = slot.position.z;

            // Move vertically so bottom touches surface
            newPos.y += offset;

            obj.transform.position = newPos;
        }
        else
        {
            obj.transform.position = slot.position;
        }

        // 🔥 Make object child of slot AFTER positioning
        obj.transform.SetParent(slot, true);
    }

    currentSlide.currentTotalWeight += item.GetWeightInGrams();
    UpdateWeightText();

    if (!currentSlide.isSolved)
        setResetButton.SetActive(true);
}
    void UpdateWeightText()
    {
        if (currentSlide.totalWeightText != null)
            currentSlide.totalWeightText.text =
                currentSlide.currentTotalWeight + "g";
    }

    // =====================================================
    // SLIDE MEMORY
    // =====================================================

    void LoadSlide(int index)
    {
        if (index < 0 || index >= slidesData.Count)
            return;

        if (currentSlide != null)
            currentSlide.savedSlots =
                new Dictionary<Transform, DraggableItem>(occupiedSlots);

        for (int i = 0; i < slidesData.Count; i++)
            //slidesData[i].displayPanel.SetActive(i == index);

        currentSlideIndex = index;
        currentSlide = slidesData[index];
currentItem = null;

        RestoreSlideState();
       

    }
void PlaySlideAnimation()
{
    if (slideAnimator == null) return;

    SnapAllObjectsToSlots();
    if (!string.IsNullOrEmpty(currentSlide.animationName))
    {
        slideAnimator.Play(currentSlide.animationName, 0, 0f);
        slideAnimator.Update(0f);
    }

    // 🔥 Force correction after animation evaluation
}



void RestoreSlideState()
{
    occupiedSlots.Clear();
    currentSlide.currentTotalWeight = 0f;

    if (currentSlide.resultImage != null)
    {
        currentSlide.resultImage.sprite = null;
        currentSlide.resultImage.gameObject.SetActive(false);
    }

    foreach (var item in items)
    {
        item.isPlaced = false;
        item.occupiedSlot = null;
        item.draggableObject.SetActive(true);
        item.draggableObject.transform.position = item.startPosition;

        if (item.objectToEnableOnCorrectDrop != null)
            item.objectToEnableOnCorrectDrop.SetActive(false);

        if (item.weightIcon != null)
            item.weightIcon.SetActive(false);
    }

    foreach (var pair in currentSlide.savedSlots)
    {
        Transform slot = pair.Key;
        DraggableItem item = pair.Value;

        occupiedSlots[slot] = item;
        item.isPlaced = true;
        item.occupiedSlot = slot;
        item.draggableObject.SetActive(false);

        if (item.objectToEnableOnCorrectDrop != null)
        {
            GameObject obj = item.objectToEnableOnCorrectDrop;
            obj.SetActive(true);

            Renderer rend = obj.GetComponentInChildren<Renderer>();
            BoxCollider slotCollider = slot.GetComponent<BoxCollider>();

            if (rend != null && slotCollider != null)
            {
                float slotTop = slotCollider.bounds.max.y;
                float objectBottom = rend.bounds.min.y;
                float offset = slotTop - objectBottom;

                Vector3 newPos = obj.transform.position;

                newPos.x = slot.position.x;
                newPos.z = slot.position.z;
                newPos.y += offset;

                obj.transform.position = newPos;
            }
            else
            {
                obj.transform.position = slot.position;
            }

            // 🔥 Make sure it is parented correctly
            obj.transform.SetParent(slot, true);
        }

        currentSlide.currentTotalWeight += item.GetWeightInGrams();
    }

    UpdateWeightText();
    HideValidationPanel();

    if (currentSlide.isSolved)
    {
        currentSlide.resultImage.gameObject.SetActive(true);
        currentSlide.resultImage.sprite = correctSprite;
        setResetButton.SetActive(false);
    }
    else
    {
        setResetButton.SetActive(occupiedSlots.Count > 0);
    }

    isResetMode = false;

    if (buttonText != null)
        buttonText.text = "SET";
}


    // =====================================================
    // SET / RESET
    // =====================================================

  public void OnSetResetClicked()
{
    float desiredWeight = currentSlide.weightIsInMilligrams
        ? currentSlide.requiredWeight / 1000f
        : currentSlide.requiredWeight;

    float currentWeight = currentSlide.currentTotalWeight;

    HideValidationPanel();

    // =========================
    // IF CURRENT MODE IS SET
    // =========================
    if (!isResetMode)
    {
        // ✅ CORRECT
        if (Mathf.RoundToInt(currentWeight) ==
            Mathf.RoundToInt(desiredWeight))
        {
            currentSlide.resultImage.gameObject.SetActive(true);
            currentSlide.resultImage.sprite = correctSprite;

            currentSlide.isSolved = true;
            oncorrectWeight?.Invoke();
            // hide button permanently
            setResetButton.SetActive(false);

            return;
        }

        // ❌ WRONG
        currentSlide.resultImage.gameObject.SetActive(true);
        currentSlide.resultImage.sprite = wrongSprite;

        if (currentWeight < desiredWeight)
            ShowValidation("Weight is LESS than target (" +
                Mathf.RoundToInt(desiredWeight) + "g)");
        else
            ShowValidation("Weight is MORE than target (" +
                Mathf.RoundToInt(desiredWeight) + "g)");

        isResetMode = true;

        if (buttonText != null)
            buttonText.text = "RESET";
    }

    // =========================
    // IF CURRENT MODE IS RESET
    // =========================
    else
    {
        ForceResetCurrentSlide();

        if (buttonText != null)
            buttonText.text = "SET";

        isResetMode = false;
    }
}


    public void ForceResetCurrentSlide()
    {
        currentSlide.savedSlots.Clear();
        currentSlide.isSolved = false;

        occupiedSlots.Clear();
        currentSlide.currentTotalWeight = 0f;

        foreach (var item in items)
        {
            item.isPlaced = false;
            item.occupiedSlot = null;
            item.draggableObject.SetActive(true);
            item.draggableObject.transform.position = item.startPosition;

            if (item.objectToEnableOnCorrectDrop != null)
                item.objectToEnableOnCorrectDrop.SetActive(false);
                if (item.weightIcon != null)
    item.weightIcon.SetActive(false);

        }

        if (currentSlide.resultImage != null)
        {
            currentSlide.resultImage.sprite = null;
            currentSlide.resultImage.gameObject.SetActive(false);
        }

        UpdateWeightText();
        HideValidationPanel();

        setResetButton.SetActive(false);

        if (buttonText != null)
            buttonText.text = "SET";

        isResetMode = false;
    }

    // =====================================================
    // UI
    // =====================================================

    void ShowValidation(string msg)
    {
        validationPanel.SetActive(true);
        validationText.text = msg;
    }

    void HideValidationPanel()
    {
        validationPanel.SetActive(false);
    }
void SnapAllObjectsToSlots()
{
    foreach (var pair in occupiedSlots)
    {
        Transform slot = pair.Key;
        DraggableItem item = pair.Value;

        if (item.objectToEnableOnCorrectDrop == null) continue;

        GameObject obj = item.objectToEnableOnCorrectDrop;

        Renderer rend = obj.GetComponentInChildren<Renderer>();
        BoxCollider slotCollider = slot.GetComponent<BoxCollider>();

        if (rend != null && slotCollider != null)
        {
            float slotTop = slotCollider.bounds.max.y;
            float objectBottom = rend.bounds.min.y;
            float offset = slotTop - objectBottom;

            Vector3 newPos = obj.transform.position;
            newPos.x = slot.position.x;
            newPos.z = slot.position.z;
            newPos.y += offset;

            obj.transform.position = newPos;
        }
        else
        {
            obj.transform.position = slot.position;
        }
    }
}
void LateUpdate()
{
    SnapAllObjectsToSlots();
}


public void NextSlide()
{
    HideValidationPanel();
    weight_gauge.SetActive(false);
    SlidePuzzleData previousSlide = currentSlide;

    LoadSlide(currentSlideIndex + 1);

    if (slideAnimator == null) return;

    if (previousSlide != null &&
        previousSlide.isSolved &&
        currentSlide != null &&
        currentSlide.isSolved &&
                                    // 🔥 NEW CONDITION
        !string.IsNullOrEmpty(currentSlide.animationName))
    {
        // ✅ All conditions true → play current animation
        slideAnimator.Play(currentSlide.animationName, 0, 0f);
        slideAnimator.Update(0f);
    }
    else
    {
        // ❌ Otherwise → Idle
        slideAnimator.Rebind();
        slideAnimator.Update(0f);
        slideAnimator.Play("Idle", 0, 0f);
    }

    //SnapAllObjectsToSlots();

    // Optional: reset click after use
  
}




  public void PreviousSlide()
{
    HideValidationPanel();
    LoadSlide(currentSlideIndex - 1);

    if (currentSlide != null &&
        currentSlide.isSolved &&
        slideAnimator != null &&
        !string.IsNullOrEmpty(currentSlide.animationName))
    {
        slideAnimator.Play(currentSlide.animationName, 0, 0f);
        slideAnimator.Update(0f);
    }
    else
    {
        slideAnimator.Rebind();
        slideAnimator.Update(0f);
    }
 SnapAllObjectsToSlots();
}


    
}
