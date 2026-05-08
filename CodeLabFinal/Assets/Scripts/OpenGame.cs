using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenGame : MonoBehaviour
{
    // 在 Inspector 面板里填入你想跳转的场景名字
    public string targetSceneName;

    public void LoadNextScene()
    {
        // 记得在 Build Settings 里把两个场景都添加进去
        SceneManager.LoadScene(targetSceneName);
    }
}