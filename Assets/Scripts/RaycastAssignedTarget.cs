using UnityEngine;
using UnityEngine.Events;

public class RaycastAssignedTarget : MonoBehaviour
{
    [Header("Assign Target Here")]
    public GameObject targetObject;

    [Header("Event To Fire")]
    public UnityEvent onTargetClicked;
    public GameObject objectoenable1;
      public GameObject objectoenable2;
    void Start()
    {
        objectoenable1.SetActive(true);
        objectoenable2.SetActive(true);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == targetObject)
                {
                    onTargetClicked?.Invoke();
                }
            }
        }
    }
}
