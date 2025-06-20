using UnityEngine;
using UnityEngine.InputSystem;

public class UserInput : MonoBehaviour
{
    public static UserInput instance;
    public static PlayerInput PlayerInput;

    public Vector2 MoveInput {  get; private set; }
    public bool ActionOneJustPressed { get; private set; }
    public bool ActionTwoJustPressed { get; private set; }
    public bool ActionThreeJustPressed { get; private set; }
    public bool ActionFourJustPressed { get; private set; }

    public bool ActionOneBeingHeld { get; private set; }
    public bool ActionTwoBeingHeld { get; private set; }
    public bool ActionThreeBeingHeld { get; private set; }
    public bool ActionFourBeingHeld { get; private set; }

    public bool ActionOneReleased { get; private set; }
    public bool ActionTwoReleased { get; private set; }
    public bool ActionThreeReleased { get; private set; }
    public bool ActionFourReleased { get; private set; }

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
        ActionOneJustPressed = _actionOne.WasPressedThisFrame();
        ActionTwoJustPressed = _actionTwo.WasPressedThisFrame();
        ActionThreeJustPressed = _actionThree.WasPressedThisFrame();
        ActionFourJustPressed = _actionFour.WasPressedThisFrame();

        ActionOneBeingHeld = _actionOne.IsPressed();
        ActionTwoBeingHeld = _actionTwo.IsPressed();
        ActionThreeBeingHeld = _actionThree.IsPressed();
        ActionFourBeingHeld = _actionFour.IsPressed();

        ActionOneReleased = _actionOne.WasReleasedThisFrame();
        ActionTwoReleased = _actionTwo.WasReleasedThisFrame();
        ActionThreeReleased =_actionThree.WasReleasedThisFrame();
        ActionFourReleased = _actionFour.WasReleasedThisFrame();

        MenuOpenCloseInput = _menuOpenCloseAction.WasPressedThisFrame();
    }
}
