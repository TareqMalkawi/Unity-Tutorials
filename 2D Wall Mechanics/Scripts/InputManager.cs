using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private InputSystem_Actions genericPlayerInput;

    private float horizontalInput;
    private float verticalInput;
    public bool DashInputPressed { get; set; }
    public bool DashInputHold { get; set; }
    public bool JumpInputPressed { get; set; }
    public bool WallClimpingInputHold { get; set; }

    private void Awake()
    {
        genericPlayerInput = new InputSystem_Actions();
        genericPlayerInput?.Player.Enable();
    }

    public float GetHorizontalInput()
    {
        return horizontalInput;
    }   
    public float GetVerticalInput()
    {
        return verticalInput;
    }

    private void Update()
    {
        horizontalInput = genericPlayerInput.Player.Move.ReadValue<Vector2>().x;
        verticalInput = genericPlayerInput.Player.Move.ReadValue<Vector2>().y;

        JumpInputPressed = genericPlayerInput.Player.Jump.WasPressedThisFrame();
        DashInputPressed = genericPlayerInput.Player.Dash.WasPressedThisFrame();
        DashInputHold = genericPlayerInput.Player.Dash.IsPressed();

        WallClimpingInputHold = genericPlayerInput.Player.WallClimb.IsPressed();
    }
}