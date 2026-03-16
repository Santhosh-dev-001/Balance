using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BalanceComponentFocusSystem : MonoBehaviour
{
    [System.Serializable]
    public class BalanceComponent
    {
        [Tooltip("All meshes that belong to this one component")]
        public List<Renderer> objectRenderers = new List<Renderer>();

        public Transform cameraPoint;
        public GameObject infoPanel;

        [HideInInspector] public List<Material> originalMats = new List<Material>();
        [HideInInspector] public bool viewed;
    }

    [Header("Camera")]
    [SerializeField] private Transform mainCamera;
    [SerializeField] private float cameraMoveSpeed = 4f;

    [Header("Highlight")]
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private float shineScrollSpeed = 0.5f;

    [Header("Components")]
    [SerializeField] private List<BalanceComponent> components = new List<BalanceComponent>();

    [Header("Page Flow Manager")]
    [SerializeField] private AnalyticalBalancePageFlowManager pageFlowManager;

    private BalanceComponent currentComponent;
    private Coroutine camMoveRoutine;
    private float shineOffset;

    private void Update()
    {
        if (currentComponent != null && highlightMaterial != null)
        {
            shineOffset += Time.deltaTime * shineScrollSpeed;
            highlightMaterial.SetTextureOffset("_EmissionMap", new Vector2(shineOffset, 0));
        }
    }

    public void FocusComponent(int index)
    {
        if (index < 0 || index >= components.Count) return;

        BalanceComponent target = components[index];

        RestoreCurrentComponent();

        if (target.originalMats.Count == 0)
        {
            foreach (var rend in target.objectRenderers)
                target.originalMats.Add(rend.sharedMaterial);
        }

        foreach (var rend in target.objectRenderers)
            rend.sharedMaterial = highlightMaterial;

        if (target.infoPanel != null)
            target.infoPanel.SetActive(true);

        currentComponent = target;

        if (!target.viewed)
            target.viewed = true;

        if (target.cameraPoint != null)
            MoveCamera(target.cameraPoint);

        CheckAllViewed();
    }

    public void ClearHighlight()
    {
        RestoreCurrentComponent();
    }

    private void RestoreCurrentComponent()
    {
        if (currentComponent == null) return;

        for (int i = 0; i < currentComponent.objectRenderers.Count; i++)
        {
            if (i < currentComponent.originalMats.Count)
                currentComponent.objectRenderers[i].sharedMaterial =
                    currentComponent.originalMats[i];
        }

        if (currentComponent.infoPanel != null)
            currentComponent.infoPanel.SetActive(false);

        currentComponent = null;
    }

    private void MoveCamera(Transform target)
    {
        if (camMoveRoutine != null)
            StopCoroutine(camMoveRoutine);

        camMoveRoutine = StartCoroutine(SmoothMove(target));
    }

    private IEnumerator SmoothMove(Transform target)
    {
        while (Vector3.Distance(mainCamera.position, target.position) > 0.02f)
        {
            mainCamera.position = Vector3.Lerp(mainCamera.position, target.position, Time.deltaTime * cameraMoveSpeed);
            mainCamera.rotation = Quaternion.Slerp(mainCamera.rotation, target.rotation, Time.deltaTime * cameraMoveSpeed);
            yield return null;
        }
    }

    private void CheckAllViewed()
    {
        foreach (var comp in components)
            if (!comp.viewed) return;

        if (pageFlowManager != null)
            pageFlowManager.UnlockNextStep();
    }
}
