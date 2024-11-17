using UnityEngine;

public class InputManager : MonoBehaviour
{
    private InputSystem_Actions genericPlayerInput;
    [HideInInspector] public bool DashInputHold { get; set; }

    private void Awake()
    {
        genericPlayerInput = new InputSystem_Actions();
        genericPlayerInput?.Player.Enable();
    }

    private void Update()
    {
        DashInputHold = genericPlayerInput.Player.Dash.IsPressed();
    }
}