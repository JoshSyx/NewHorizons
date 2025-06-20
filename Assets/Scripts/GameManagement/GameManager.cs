using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] private GameObject _player;

    public bool isGameOver;
    public bool isGamePaused;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void Update()
    {
        if (_player == null) {
            if (!isGameOver)
            {
                GameOver();
            }
            return;
        }

        if (UserInput.instance.MenuOpenCloseInput && !isGameOver)
        {
            if (isGamePaused) UnPause();
            else Pause();
        }
    }

    public void Pause()
    {
        PauseManager.instance.PauseGame();
        MenuManager.instance.Pause();
        ActionMapManager.instance.SwitchUI();
    }

    public void UnPause()
    {
        PauseManager.instance.UnpauseGame();
        MenuManager.instance.Unpause();
        ActionMapManager.instance.SwitchGame();
    }

    public void GameOver()
    {
        isGameOver = true;
        PauseManager.instance.PauseGame();
        MenuManager.instance.OnGameOver();
        ActionMapManager.instance.SwitchUI();
    }
}
