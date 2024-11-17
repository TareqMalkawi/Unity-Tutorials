using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [HideInInspector] private InputSystem_Actions genericPlayerInput;

    [HideInInspector] private float horizontalInput;
    [HideInInspector] private float verticalInput;
    [HideInInspector] public bool DashInputPressed { get; set; }
    [HideInInspector] public bool DashInputHold { get; set; }
    [HideInInspector] public bool JumpInputPressed { get; set; }

    private Dash dash;

    private void Awake()
    {
        dash = GetComponent<Dash>();

        genericPlayerInput = new InputSystem_Actions();
        genericPlayerInput?.Player.Enable();

        //genericPlayerInput.Player.Dash.canceled += Dash_canceled;
        //genericPlayerInput.Player.Dash.performed += Dash_performed;
    }

    private void Dash_canceled(InputAction.CallbackContext obj)
    {
        DashInputHold = false;
    }
    private void Dash_performed(InputAction.CallbackContext obj)
    {
        var control = obj.control;
        if (control != null)
        {
            ButtonControl button = control as ButtonControl;
            if (button != null && button.wasPressedThisFrame)
            {
                if (dash)
                {
                    // dash.DashOnInit();
                }

                DashInputHold = true;
            }
        }
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
    }
}