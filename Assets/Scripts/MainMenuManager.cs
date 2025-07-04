using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance;

    [Header("Menu Objects")]
    [SerializeField] private GameObject _mainMenuCanvasGO;
    [SerializeField] private GameObject _settingsMenuCanvasGO;
    [SerializeField] private GameObject _gamepadControlsMenuCanvasGO;
    [SerializeField] private GameObject _keyboardControlsMenuCanvasGO;

    [Header("First Selected Options")]
    [SerializeField] private GameObject _mainMenuFirst;
    [SerializeField] private GameObject _settingsMenuFirst;
    [SerializeField] private GameObject _gamepadControlMenuFirst;
    [SerializeField] private GameObject _keyboardControlMenuFirst;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        _mainMenuCanvasGO.SetActive(true);
        _settingsMenuCanvasGO.SetActive(false);
        _gamepadControlsMenuCanvasGO.SetActive(false);
        _keyboardControlsMenuCanvasGO.SetActive(false);
    }

    #region Canvas Activations/Deactivations
    public void OpenMainMenu()
    {
        _mainMenuCanvasGO.SetActive(true);
        _settingsMenuCanvasGO.SetActive(false);
        _gamepadControlsMenuCanvasGO.SetActive(false);
        _keyboardControlsMenuCanvasGO.SetActive(false);

        EventSystem.current.SetSelectedGameObject(_mainMenuFirst);
    }

    private void OpenSettingsMenuHandle()
    {
        _settingsMenuCanvasGO.SetActive(true);
        _mainMenuCanvasGO.SetActive(false);
        _gamepadControlsMenuCanvasGO.SetActive(false);
        _keyboardControlsMenuCanvasGO.SetActive(false);

        EventSystem.current.SetSelectedGameObject(_settingsMenuFirst);
    }
    #endregion

    #region Main Menu Button Actions

    public void OnSettingsPress()
    {
        OpenSettingsMenuHandle();
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

        EventSystem.current.SetSelectedGameObject(_gamepadControlMenuFirst);
    }

    public void OnKeyboardControlsPress()
    {
        _mainMenuCanvasGO.SetActive(false);
        _settingsMenuCanvasGO.SetActive(false);
        _gamepadControlsMenuCanvasGO.SetActive(false);
        _keyboardControlsMenuCanvasGO.SetActive(true);

        EventSystem.current.SetSelectedGameObject(_keyboardControlMenuFirst);
    }
    public void OnControlsBackPress()
    {
        OpenSettingsMenuHandle();
    }
    #endregion
}
