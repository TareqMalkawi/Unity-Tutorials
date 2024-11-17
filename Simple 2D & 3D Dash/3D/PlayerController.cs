using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Reference to the horizontal input.
    private float horizontalInput;

    // Reference to the vertical input.
    private float verticalInput;

    // Reference to the calculated movement direction.
    private Vector3 movement;

    // Used to simulate gravity.
    private float verticalSpeed;

    // Store the total velocity including the vertical one.
    private Vector3 velocity;

    [Header("Settings")]
    [SerializeField] private float characterMovementSpeed;
    [SerializeField] private float rotationSpeed;

    [Header("Physics")]
    [SerializeField] private float checkSphereRadius;
    [SerializeField] private Vector3 checkSphereOffset;
    [SerializeField] private LayerMask groundLayer;

    private CameraController cameraController;

    // Reference to the Animator component.
    private Animator characterAnimator;

    // Reference to the CharacterController component.
    private CharacterController characterController;

    private Quaternion desiredRotation;

    private Vector3 desiredMovementDir;

    public bool CanMove { get; set; }

    private void Start()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
        characterAnimator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        CanMove = true;
    }

    private void Update()
    {
        if (!CanMove) return;

        // Get horizontal & vertical input.
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        // Calculate the movement speed from the input. We use this to let the camera only follows the player when he/she moves.
        var movementSpeed = Mathf.Clamp01(Mathf.Abs(horizontalInput) + Mathf.Abs(verticalInput));

        // Calculate the movement direction, and then normalize it. We want direction so we don't care about the magnitude.
        movement = new Vector3(horizontalInput, 0.0f, verticalInput).normalized;

        desiredMovementDir = cameraController.YRotation * movement;

        if (IsGrounded())
        {
            // Add a little bit of downward force to push him/her to the ground.
            verticalSpeed = -0.5f;
        }
        else
        {
            // Simulate gravity.
            verticalSpeed += Physics.gravity.y * Time.deltaTime;
        }

        // Store the total velocity including the vertical one.
        velocity = desiredMovementDir * characterMovementSpeed;
        velocity.y = verticalSpeed;

        if (movementSpeed > 0.0f)
        {
            desiredRotation = Quaternion.LookRotation(desiredMovementDir);
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation
            , rotationSpeed * Time.deltaTime);

        //if(characterAnimator)
        //{
        //    // Set the MovementSpeed parameter in animator window, so the blend tree would starts.
        //    characterAnimator.SetFloat("MovementSpeed", movementSpeed, 0.2f, Time.deltaTime);
        //}

        // Use the move method on the character controller to move the player.
        characterController.Move(velocity * Time.deltaTime);    
    }

    public bool IsGrounded()
    {
        return Physics.CheckSphere(transform.TransformPoint(checkSphereOffset),checkSphereRadius,groundLayer);
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1.0f,0.0f,0.0f,0.3f);
        Gizmos.DrawSphere(transform.TransformPoint(checkSphereOffset), checkSphereRadius);
    }

    public Vector3 GetMovementDirection() => desiredMovementDir;
}