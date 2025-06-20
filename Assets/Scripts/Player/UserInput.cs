using UnityEngine;
using UnityEngine.InputSystem;

public class UserInput : MonoBehaviour
{
    public static UserInput instance;
    public static PlayerInput PlayerInput;

    public Vector2 MoveInput {  get; private set; }
    public bool actionOneJustPressed { get; private set; }
    public bool actionTwoJustPressed { get; private set; }
    public bool actionThreeJustPressed { get; private set; }
    public bool actionFourJustPressed { get; private set; }

    public bool actionOneBeingHeld { get; private set; }
    public bool actionTwoBeingHeld { get; private set; }
    public bool actionThreeBeingHeld { get; private set; }
    public bool actionFourBeingHeld { get; private set; }

    public bool actionOneReleased { get; private set; }
    public bool actionTwoReleased { get; private set; }
    public bool actionThreeReleased { get; private set; }
    public bool actionFourReleased { get; private set; }

    public bool MenuOpenCloseInput { get; private set; }


    private InputAction _moveAction;
    private InputAction _actionOne;
    private InputAction _actionTwo;
    private InputAction _actionThree;
    private InputAction _actionFour;
    private InputAction _menuOpenCloseAction;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        PlayerInput = GetComponent<PlayerInput>();

        SetupInputActions();
    }

    private void Update()
    {
        Debug.Log("Current Control Scheme: " + PlayerInput.currentControlScheme);
        UpdateInputs();
    }
    public void SetupInputActions()
    {
        _moveAction = PlayerInput.actions["Move"];
        _actionOne = PlayerInput.actions["Action One"];
        _actionTwo = PlayerInput.actions["Action Two"];
        _actionThree = PlayerInput.actions["Action Three"];
        _actionFour = PlayerInput.actions["Action Four"];
        _menuOpenCloseAction = PlayerInput.actions["Menu Open Close"];
    }

    private void UpdateInputs()
    {
        MoveInput = _moveAction.ReadValue<Vector2>();
        actionOneJustPressed = _actionOne.WasPressedThisFrame();
        actionTwoJustPressed = _actionTwo.WasPressedThisFrame();
        actionThreeJustPressed = _actionThree.WasPressedThisFrame();
        actionFourJustPressed = _actionFour.WasPressedThisFrame();

        actionOneBeingHeld = _actionOne.IsPressed();
        actionTwoBeingHeld = _actionTwo.IsPressed();
        actionThreeBeingHeld = _actionThree.IsPressed();
        actionFourBeingHeld = _actionFour.IsPressed();

        actionOneReleased = _actionOne.WasReleasedThisFrame();
        actionTwoReleased = _actionTwo.WasReleasedThisFrame();
        actionThreeReleased =_actionThree.WasReleasedThisFrame();
        actionFourReleased = _actionFour.WasReleasedThisFrame();

        MenuOpenCloseInput = _menuOpenCloseAction.WasPressedThisFrame();
    }
}
