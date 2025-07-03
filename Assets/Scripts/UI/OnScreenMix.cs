using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

[AddComponentMenu("Input/On-Screen Mix Joystick")]
public class OnScreenMix : OnScreenControl, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField]
    private InputBinding m_ControlBinding;

    [SerializeField]
    private InputBinding m_PressBinding;

    public RectTransform stickArea;
    public float stickRange = 100f;

    [Header("Visual Smoothing")]
    public float visualDeadzone = 5f; // pixels, small movements snap to center visually
    public float handleSmoothSpeed = 10f; // how fast handle moves visually

    private RectTransform m_RectTransform;
    private Vector2 m_StartPosition;
    private Vector2 m_PointerDownPosition;
    private Vector2 m_TargetPosition;
    private bool m_IsPressed = false;

    private InputControl m_PressControl;

    protected override void OnEnable()
    {
        base.OnEnable();

        m_RectTransform = GetComponent<RectTransform>();
        m_StartPosition = m_RectTransform.anchoredPosition;
        m_TargetPosition = m_StartPosition;

        if (!string.IsNullOrEmpty(m_PressBinding.effectivePath))
            m_PressControl = InputSystem.FindControl(m_PressBinding.effectivePath);

        if (stickArea == null)
            Debug.LogError("stickArea is not assigned! Please assign it in the inspector.");
    }

    private void Update()
    {
        if (m_RectTransform == null)
            return;

        // Smoothly interpolate handle position towards target position
        m_RectTransform.anchoredPosition = Vector2.Lerp(m_RectTransform.anchoredPosition, m_TargetPosition, Time.deltaTime * handleSmoothSpeed);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        m_IsPressed = true;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            stickArea, eventData.position, eventData.pressEventCamera, out m_PointerDownPosition);
        UpdateStick(eventData);

        // Send press signal
        if (m_PressControl != null)
            SendValueToControl(1f, m_PressControl);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        m_IsPressed = false;
        m_TargetPosition = m_StartPosition; // Reset target position smoothly
        SendValueToControl(Vector2.zero);

        // Send release signal
        if (m_PressControl != null)
            SendValueToControl(0f, m_PressControl);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!m_IsPressed)
            return;

        UpdateStick(eventData);
    }

    private void UpdateStick(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            stickArea, eventData.position, eventData.pressEventCamera, out var currentPosition);

        var delta = currentPosition - m_PointerDownPosition;
        delta = Vector2.ClampMagnitude(delta, stickRange);

        // Visual deadzone: snap to center if close enough
        if (delta.magnitude < visualDeadzone)
            delta = Vector2.zero;

        m_TargetPosition = m_StartPosition + delta;

        var normalized = delta / stickRange;
        SendValueToControl(normalized);
    }

    protected override string controlPathInternal
    {
        get => m_ControlBinding.effectivePath;
        set => m_ControlBinding.overridePath = value;
    }

    // Overload for Vector2 control (stick movement)
    private void SendValueToControl(Vector2 value)
    {
        if (control == null)
            return;

        if (control is Vector2Control vector2Control)
        {
            InputSystem.QueueDeltaStateEvent(vector2Control, value);
        }
    }

    // Overload for float control (press as button)
    private void SendValueToControl(float value, InputControl control)
    {
        if (control == null)
            return;

        if (control is ButtonControl button)
        {
            InputSystem.QueueDeltaStateEvent(button, value);
        }
    }
}
