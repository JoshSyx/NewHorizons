using UnityEngine;

public class SceneManagerController : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        FadeManager.instance.LoadScene(sceneName);
    }

    public void ReloadCurrentScene()
    {
        FadeManager.instance.ReloadScene();
    }

    public void LoadNextScene()
    {
        FadeManager.instance.LoadNextScene();
    }

    public void QuitGame()
    {
        Debug.Log("Quitting the game...");
        Application.Quit();
    }
}
