using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenGame : MonoBehaviour
{
    public string targetSceneName;

    public void LoadNextScene()
    {
        SceneManager.LoadScene(targetSceneName);
    }
}