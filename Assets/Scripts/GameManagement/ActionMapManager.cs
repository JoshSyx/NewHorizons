using UnityEngine;

public class ActionMapManager : MonoBehaviour
{
    public static ActionMapManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void SwitchUI()
    {
        UserInput.PlayerInput.SwitchCurrentActionMap("UI");
        UserInput.instance.SetupInputActions();
        Debug.Log(UserInput.PlayerInput.currentActionMap.name);
    }

    public void SwitchGame()
    {
        UserInput.PlayerInput.SwitchCurrentActionMap("Game");
        UserInput.instance.SetupInputActions();
        Debug.Log(UserInput.PlayerInput.currentActionMap.name);
    }
}
