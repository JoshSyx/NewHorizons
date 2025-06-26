using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Shrine : MonoBehaviour
{
    public UnityEvent onShrineActivated;
    
    [SerializeField]
    private bool activateOnCollision = true;
    [SerializeField]
    [HideIf("activateOnCollision")]
    private InputActionReference activateAction;
    [SerializeField]
    private bool healOnActivation = true;
    [SerializeField]
    private ShrinesUI shrineUI;

    private bool _shrineActivated;
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        Heal(other);
        shrineUI.ShowUI(this);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player") || activateOnCollision) return;

        if (activateAction.action.triggered && !_shrineActivated)
        {
            shrineUI.ShowUI(this);
            Heal(other);
            _shrineActivated = true;
        }
    }

    private void Heal(Collider other)
    {
        if (other.gameObject.TryGetComponent(out Health health) && healOnActivation)
        {
            health.Heal(1);
        }
    }
}
