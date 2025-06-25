using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void PauseGame()
    {
        if (!GameManager.Instance.isGamePaused)
        {
            GameManager.Instance.isGamePaused = true;
            Time.timeScale = 0f;
        }
    }

    public void UnpauseGame()
    {
        if (GameManager.Instance.isGamePaused)
        {
            GameManager.Instance.isGamePaused = false;
            Time.timeScale = 1f;
        }
    }
}
