


using UnityEngine;
using UnityEngine.EventSystems;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [Header("Menu Objects")]
    [SerializeField] private GameObject _mainMenuCanvasGO;
    [SerializeField] private GameObject _settingsMenuCanvasGO;
    [SerializeField] private GameObject _gamepadControlsMenuCanvasGO;
    [SerializeField] private GameObject _keyboardControlsMenuCanvasGO;
    [SerializeField] private GameObject _gameOverMenuCanvasGO;

    [Header("First Selected Options")]
    [SerializeField] private GameObject _mainMenuFirst;
    [SerializeField] private GameObject _settingsMenuFirst;
    [SerializeField] private GameObject _gamepadControlMenuFirst;
    [SerializeField] private GameObject _keyboardControlMenuFirst;
    [SerializeField] private GameObject _GameOverMenuFirst;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        _mainMenuCanvasGO.SetActive(false);
        _settingsMenuCanvasGO.SetActive(false);
        _gamepadControlsMenuCanvasGO.SetActive(false);
        _keyboardControlsMenuCanvasGO.SetActive(false);
        _gameOverMenuCanvasGO.SetActive(false);
    }

    #region Pause/Unpause Functions


    public void Pause()
    {
        OpenMainMenu();
    }

    public void Unpause()
    {
        CloseAllMenus();
    }

    #endregion

    #region Canvas Activations/Deactivations
    public void OpenMainMenu()
    {
        _mainMenuCanvasGO.SetActive(true);
        _settingsMenuCanvasGO.SetActive(false);
        _gamepadControlsMenuCanvasGO.SetActive(false);
        _keyboardControlsMenuCanvasGO.SetActive(false);
        _gameOverMenuCanvasGO.SetActive(false);

        EventSystem.current.SetSelectedGameObject(_mainMenuFirst);
    }

    private void OpenSettingsMenuHandle()
    {
        _settingsMenuCanvasGO.SetActive(true);
        _mainMenuCanvasGO.SetActive(false);
        _gamepadControlsMenuCanvasGO.SetActive(false);
        _keyboardControlsMenuCanvasGO.SetActive(false);
        _gameOverMenuCanvasGO.SetActive(false);

        EventSystem.current.SetSelectedGameObject(_settingsMenuFirst);
    }

    public void CloseAllMenus()
    {
        _mainMenuCanvasGO.SetActive(false);
        _settingsMenuCanvasGO.SetActive(false);
        _gamepadControlsMenuCanvasGO.SetActive(false);
        _keyboardControlsMenuCanvasGO.SetActive(false);
        _gameOverMenuCanvasGO.SetActive(false);

        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnGameOver()
    {
        _mainMenuCanvasGO.SetActive(false);
        _settingsMenuCanvasGO.SetActive(false);
        _gamepadControlsMenuCanvasGO.SetActive(false);
        _keyboardControlsMenuCanvasGO.SetActive(false);
        _gameOverMenuCanvasGO.SetActive(true);

        EventSystem.current.SetSelectedGameObject(_GameOverMenuFirst);
    }
    #endregion

    #region Main Menu Button Actions

    public void OnSettingsPress()
    {
        OpenSettingsMenuHandle();
    }

    public void OnResumePress()
    {
        GameManager.Instance.UnPause();
    }
    #endregion

    #region Settings Menu Button Actions
    public void OnSettingsBackPress()
    {
        OpenMainMenu();
    }

    public void OnGamepadControlsPress()
    {
        _mainMenuCanvasGO.SetActive(false);
        _settingsMenuCanvasGO.SetActive(false);
        _gamepadControlsMenuCanvasGO.SetActive(true);
        _keyboardControlsMenuCanvasGO.SetActive(false);
        _gameOverMenuCanvasGO.SetActive(false);

        EventSystem.current.SetSelectedGameObject(_gamepadControlMenuFirst);
    }

    public void OnKeyboardControlsPress()
    {
        _mainMenuCanvasGO.SetActive(false);
        _settingsMenuCanvasGO.SetActive(false);
        _gamepadControlsMenuCanvasGO.SetActive(false);
        _keyboardControlsMenuCanvasGO.SetActive(true);
        _gameOverMenuCanvasGO.SetActive(false);

        EventSystem.current.SetSelectedGameObject(_keyboardControlMenuFirst);
    }
    public void OnControlsBackPress()
    {
        OpenSettingsMenuHandle();
    }
    #endregion
}