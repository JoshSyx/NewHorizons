using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager instance;
    public MenuManager MenuManager;

    public bool isPaused;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    private void Update()
    {
        if (UserInput.instance.MenuOpenCloseInput)
        {
            if (!isPaused)
            {
                PauseGame();
            }
            else
            {
                UnpauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        MenuManager.OpenMainMenu();
        UserInput.PlayerInput.SwitchCurrentActionMap("UI");
        UserInput.instance.SetupInputActions();
        Debug.Log(UserInput.PlayerInput.currentActionMap.name);


    }

    public void UnpauseGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        MenuManager.CloseAllMenus();
        UserInput.PlayerInput.SwitchCurrentActionMap("Game");
        UserInput.instance.SetupInputActions();
        Debug.Log(UserInput.PlayerInput.currentActionMap.name);
    }
}
