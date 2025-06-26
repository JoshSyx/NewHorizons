using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [HideInInspector]
    public GameObject _player;

    public bool isGameOver;
    public bool isGamePaused;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        _player = GameObject.FindGameObjectWithTag("Player");
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

        if (UserInput.Instance.MenuOpenCloseInput && !isGameOver)
        {
            if (isGamePaused) UnPause();
            else Pause();
        }
    }

    public void Pause()
    {
        PauseManager.Instance.PauseGame();
        MenuManager.Instance.Pause();
        ActionMapManager.instance.SwitchUI();
    }

    public void UnPause()
    {
        PauseManager.Instance.UnpauseGame();
        MenuManager.Instance.Unpause();
        ActionMapManager.instance.SwitchGame();
    }

    public void GameOver()
    {
        isGameOver = true;
        PauseManager.Instance.PauseGame();
        MenuManager.Instance.OnGameOver();
        ActionMapManager.instance.SwitchUI();
    }
}
