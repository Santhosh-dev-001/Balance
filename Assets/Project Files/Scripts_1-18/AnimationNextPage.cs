using UnityEngine;

public class AnimationNextPage : MonoBehaviour
{
    [SerializeField] private AnalyticalBalancePageFlowManager pageFlowManager;
    public GameObject salt;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void  OnAnimationComplet()
    {
        // If salt exists ? enable it
           pageFlowManager.UnlockNextStep();
        
    }
   public void ObjectToEnable()
    {
        salt.SetActive(true);
    }
}
