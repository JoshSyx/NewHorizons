using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager instance;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void PauseGame()
    {
        if (!GameManager.instance.isGamePaused)
        {
            GameManager.instance.isGamePaused = true;
            Time.timeScale = 0f;
        }
    }

    public void UnpauseGame()
    {
        if (GameManager.instance.isGamePaused)
        {
            GameManager.instance.isGamePaused = false;
            Time.timeScale = 1f;
        }
    }
}
