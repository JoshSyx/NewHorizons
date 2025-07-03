using UnityEngine;
using UnityEngine.InputSystem;

public class UserInput : MonoBehaviour
{
    public static UserInput Instance;
    public static PlayerInput PlayerInput;

    public bool DashTriggered { get; private set; }
    public Vector2 DashDirection { get; private set; }

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

    [Header("Double Tap Dash Settings")]
    [SerializeField] private float doubleTapThreshold = 0.3f;
    [SerializeField] private float dashDirectionThreshold = 0.7f;

    private enum DashTapState
    {
        WaitingForFirstTap,
        WaitingForRelease,
        WaitingForSecondTap
    }

    private DashTapState doubleTapState = DashTapState.WaitingForFirstTap;
    private float lastTapTime = -1f;
    private Vector2 lastTapDirection = Vector2.zero;

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

        DashTriggered = false;

        // --- Updated Double Tap Dash Logic ---
        float now = Time.time;

        if (AimInput.magnitude >= dashDirectionThreshold)
        {
            Vector2 currentDirection = AimInput.normalized;

            switch (doubleTapState)
            {
                case DashTapState.WaitingForFirstTap:
                    doubleTapState = DashTapState.WaitingForRelease;
                    lastTapTime = now;
                    lastTapDirection = currentDirection;
                    break;

                case DashTapState.WaitingForSecondTap:
                    if ((now - lastTapTime) <= doubleTapThreshold &&
                        Vector2.Dot(currentDirection, lastTapDirection) > 0.9f)
                    {
                        DashTriggered = true;
                        DashDirection = currentDirection;

                        // Reset
                        doubleTapState = DashTapState.WaitingForFirstTap;
                        lastTapTime = -1f;
                        lastTapDirection = Vector2.zero;
                    }
                    else
                    {
                        // Restart as new first tap
                        doubleTapState = DashTapState.WaitingForRelease;
                        lastTapTime = now;
                        lastTapDirection = currentDirection;
                    }
                    break;
            }
        }
        else
        {
            if (doubleTapState == DashTapState.WaitingForRelease)
            {
                doubleTapState = DashTapState.WaitingForSecondTap;
            }
            else if (doubleTapState == DashTapState.WaitingForSecondTap &&
                     (now - lastTapTime) > doubleTapThreshold)
            {
                // Timeout
                doubleTapState = DashTapState.WaitingForFirstTap;
                lastTapTime = -1f;
                lastTapDirection = Vector2.zero;
            }
        }
    }

    public Vector3 GetWorldDashDirection()
    {
        if (Camera.main == null) return Vector3.zero;

        Vector2 aimDir = DashDirection.normalized;
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        return (camForward * aimDir.y + camRight * aimDir.x).normalized;
    }
}
