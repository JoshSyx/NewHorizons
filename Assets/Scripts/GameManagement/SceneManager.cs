using UnityEngine;

public class SceneManagerController : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        FadeManager.Instance.LoadScene(sceneName);
    }

    public void ReloadCurrentScene()
    {
        FadeManager.Instance.ReloadScene();
    }

    public void LoadNextScene()
    {
        FadeManager.Instance.LoadNextScene();
    }

    public void QuitGame()
    {
        Debug.Log("Quitting the game...");
        Application.Quit();
    }
}
