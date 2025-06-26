using UnityEngine;
using UnityEngine.InputSystem;

public class UserInput : MonoBehaviour
{
    public static UserInput Instance;
    public static PlayerInput PlayerInput;

    public Vector2 MoveInput { get; private set; }
    public Vector2 AimInput { get; private set; }

    public bool ActionOneJustPressed { get; private set; }
    public bool ActionTwoJustPressed { get; private set; }
    public bool ActionThreeJustPressed { get; private set; }
    public bool ActionFourJustPressed { get; private set; }
    public bool ActionFiveJustPressed { get; private set; }
    public bool ActionSixJustPressed { get; private set; }
    public bool LeftTriggerJustPressed { get; private set; }


    public bool ActionOneBeingHeld { get; private set; }
    public bool ActionTwoBeingHeld { get; private set; }
    public bool ActionThreeBeingHeld { get; private set; }
    public bool ActionFourBeingHeld { get; private set; }
    public bool ActionFiveBeingHeld { get; private set; }
    public bool ActionSixBeingHeld { get; private set; }
    public bool LeftTriggerBeingHeld { get; private set; }

    public bool ActionOneReleased { get; private set; }
    public bool ActionTwoReleased { get; private set; }
    public bool ActionThreeReleased { get; private set; }
    public bool ActionFourReleased { get; private set; }
    public bool ActionFiveReleased { get; private set; }
    public bool ActionSixReleased { get; private set; }
    public bool LeftTriggerReleased { get; private set; }

    public bool MenuOpenCloseInput { get; private set; }

    private InputAction _moveAction;
    private InputAction _aimAction;
    private InputAction _actionOne;
    private InputAction _actionTwo;
    private InputAction _actionThree;
    private InputAction _actionFour;
    private InputAction _actionFive;
    private InputAction _actionSix;
    private InputAction _leftTrigger;
    private InputAction _menuOpenCloseAction;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

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
        _aimAction = PlayerInput.actions["Aim"];
        _actionOne = PlayerInput.actions["Action One"];
        _actionTwo = PlayerInput.actions["Action Two"];
        _actionThree = PlayerInput.actions["Action Three"];
        _actionFour = PlayerInput.actions["Action Four"];
        _actionFive = PlayerInput.actions["Action Five"];
        _actionSix = PlayerInput.actions["Action Six"];
        _leftTrigger = PlayerInput.actions["Trigger"];
        _menuOpenCloseAction = PlayerInput.actions["Menu Open Close"];
    }

    private void UpdateInputs()
    {
        Debug.Log(LeftTriggerBeingHeld);
        MoveInput = _moveAction.ReadValue<Vector2>();
        AimInput = _aimAction.ReadValue<Vector2>();

        ActionOneJustPressed = _actionOne.WasPressedThisFrame();
        ActionTwoJustPressed = _actionTwo.WasPressedThisFrame();
        ActionThreeJustPressed = _actionThree.WasPressedThisFrame();
        ActionFourJustPressed = _actionFour.WasPressedThisFrame();
        ActionFiveJustPressed = _actionFive.WasPressedThisFrame();
        ActionSixJustPressed = _actionSix.WasPressedThisFrame();
        LeftTriggerJustPressed = _leftTrigger.WasPressedThisFrame();

        ActionOneBeingHeld = _actionOne.IsPressed();
        ActionTwoBeingHeld = _actionTwo.IsPressed();
        ActionThreeBeingHeld = _actionThree.IsPressed();
        ActionFourBeingHeld = _actionFour.IsPressed();
        ActionFiveBeingHeld = _actionFive.IsPressed();
        ActionSixBeingHeld = _actionSix.IsPressed();
        LeftTriggerBeingHeld = _leftTrigger.IsPressed();

        ActionOneReleased = _actionOne.WasReleasedThisFrame();
        ActionTwoReleased = _actionTwo.WasReleasedThisFrame();
        ActionThreeReleased = _actionThree.WasReleasedThisFrame();
        ActionFourReleased = _actionFour.WasReleasedThisFrame();
        ActionFiveReleased = _actionFive.WasReleasedThisFrame();
        ActionSixReleased = _actionSix.WasReleasedThisFrame();
        LeftTriggerReleased = _leftTrigger.WasReleasedThisFrame();

        MenuOpenCloseInput = _menuOpenCloseAction.WasPressedThisFrame();
    }
}
