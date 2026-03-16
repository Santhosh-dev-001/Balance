using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneIndexNavigator : MonoBehaviour
{
    public void LoadTargetScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}
