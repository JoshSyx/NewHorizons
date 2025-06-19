using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : Combat
{
    private int abilityIndex = 0;
    public void OnAttack(InputAction.CallbackContext context)
    {
        abilityIndex = context.ReadValue<int>();
    }
}
