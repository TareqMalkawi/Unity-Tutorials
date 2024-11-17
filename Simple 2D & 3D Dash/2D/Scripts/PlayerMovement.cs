using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Raycast Ground Check Properties")]    
    [SerializeField] private Vector3 groundCheckStartPos;
    [SerializeField] private Vector3 groundCheckRaycastOffset;

    [SerializeField] private float raycastGroundChackLineLength;

    [SerializeField] private LayerMask groundCheckLayer;

    private bool playerGrounded;
    public bool PlayerGrounded => playerGrounded;

    private bool canMove;
    public bool CanMove { get { return canMove; } set { canMove = value; } }

    private Dash dash;

    // Reference to Rigidbody2D component.
    private Rigidbody2D myBody;
    public Rigidbody2D MyBody => myBody;

    // Reference to Animator component.
    private Animator playerAnimator;
    private InputManager inputManager;

    // Determine player direction.
    private bool isFacingRight;

    // Used to control the linear drag & gravity properties in the rigid body component
    // to prevent player sliding.
    private enum PlayerMovementState
    {
        Start,
        PlanToStop,
        Stop
    }
    private PlayerMovementState playerMovementState;

    [Header("Movement")]
    // Initial normal movement speed determined by user.
    [SerializeField] private float movementSpeed;

    // Initial max normal movement speed determined by user.
    [SerializeField] private float maxMovementSpeed;

    // Normal movement speed in air determined by user.
    [SerializeField] private float horizontalSpeedInAir;

    // Holds the force applied to grounded character.
    private Vector2 forceToAdd;

    // Holds the force applied to character in mid-air.
    private Vector2 forceInAir;

    [Header("Physics")]
    // Initial gravity of the rigid body, (0.5f) is a good value.
    [SerializeField] private float initialGravity;

    // Control the gravity when dealing with different jumping mechanics.
    [SerializeField] private float gravityMultiplier;

    // Initial linear drag of the rigid body, (0.05f) is a good value. 
    [SerializeField] private float initialLinearDrag;

    // Control the linear drag to prevent player from sliding.
    [SerializeField] private float linearDrag;

    [field: Space]
    [field:SerializeField] public bool IsPlayerAnimated { get; set; }

    // Normal jump force determined by user.
    [SerializeField] private float normalJumpForce;
    private Vector2 jumpForce;

    // Determine if character can jump this frame or not.
    private bool canJump;

    // Determine if character is falling this frame.
    // To allow the animator controller to play the falling animation.
    private bool isFalling;

    private void Start()
    {
        myBody = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponentInChildren<Animator>();
        inputManager = GetComponent<InputManager>();
        dash = GetComponent<Dash>();

        isFacingRight = true;
        canMove = true;

        jumpForce = normalJumpForce * Vector2.up;
    }

    private void Update()
    {
        PrepareJumpMechanics();
    }

    private void FixedUpdate()
    {
        CheckPlayerGrounded();

        Movement();
        HandlePhysics();

        HandleDifferentJumpMechanics();
    }

    private void LateUpdate()
    {
        HandlePlayerAnimations();
    }

    private void CheckPlayerGrounded()
    {
        // Ignore player from the physics queries.
        Physics2D.queriesStartInColliders = false;

        playerGrounded = Physics2D.Raycast(
            transform.TransformPoint(groundCheckStartPos) + groundCheckRaycastOffset,
                    Vector2.down, raycastGroundChackLineLength, groundCheckLayer) ||

            Physics2D.Raycast(
                transform.TransformPoint(groundCheckStartPos) - groundCheckRaycastOffset,
                    Vector2.down, raycastGroundChackLineLength, groundCheckLayer
        );
    }

    public void Movement()
    {
        if (!canMove) return;

        // Grounded movement.
        if (playerGrounded)
        {
            // Get the direction of movement based on user input and calculate the final force to be added.
            forceToAdd = inputManager.GetHorizontalInput() * movementSpeed * Vector2.right;

            // Add force each frame. Independent frame rate.
            myBody.AddForce(forceToAdd * Time.fixedDeltaTime, ForceMode2D.Force);

            // Player started moving.
            if (Mathf.Abs(inputManager.GetHorizontalInput()) > 0.0f)
                playerMovementState = PlayerMovementState.Start;

            // Flip the character If there is a change in direction while the player is facing the other one. 
            if ((inputManager.GetHorizontalInput() > 0.0f && !isFacingRight) ||
                            inputManager.GetHorizontalInput() < 0.0f && isFacingRight)
            {
                FlipCharacter();
            }
        }
        // Movement in air.
        else
        {
            if (inputManager.GetHorizontalInput() != 0.0f)
            {

                // Get the direction of movement in air based on user input and calculate the final force to be added.
                forceInAir = new Vector2(inputManager.GetHorizontalInput() * horizontalSpeedInAir, 0.0f);

                // Add force each frame. Independent frame rate.
                myBody.AddForce(forceInAir * Time.fixedDeltaTime, ForceMode2D.Force);
            }

            // Flip the character If there is a change in direction while the player is facing the other one. 
            if ((inputManager.GetHorizontalInput() > 0.0f && !isFacingRight) ||
                      inputManager.GetHorizontalInput() < 0.0f && isFacingRight)
            {
                FlipCharacter();
            }
        }

        // Limit the the X component of the velocity. So player won't exceed this maximum speed value.
        if (Mathf.Abs(myBody.linearVelocity.x) > (maxMovementSpeed * Time.fixedDeltaTime))
            myBody.linearVelocity = new Vector2(Mathf.Sign(myBody.linearVelocity.x) * maxMovementSpeed * Time.fixedDeltaTime,
                myBody.linearVelocity.y);
    }

    private void HandlePhysics()
    {
        if (playerGrounded)
        {
            // Player just started moving. Initial gravity scale and linear drag must be set.
            if (playerMovementState == PlayerMovementState.Start)
            {
                InitializePhysicsProperties();
            }

            TacklePlayerSliding();
        }
    }

    private void TacklePlayerSliding()
    {
        bool changedDir = (inputManager.GetHorizontalInput() > 0 && myBody.linearVelocity.x < 0)
                          || (inputManager.GetHorizontalInput() < 0 && myBody.linearVelocity.x > 0);

        // Player started slowing down.
        if (Mathf.Abs(inputManager.GetHorizontalInput()) < 0.3f || changedDir)
            playerMovementState = PlayerMovementState.PlanToStop;

        // Add linear drag to stop character sliding.
        if (playerMovementState == PlayerMovementState.PlanToStop)
        {
            myBody.linearDamping = linearDrag;

            // If player velocity closing to zero, then player will be considered in stop state.
            if (Mathf.Abs(myBody.linearVelocity.x) <= 0.1f)
                playerMovementState = PlayerMovementState.Stop;
        }

        // Return player to his/her initial state, so player can move again smoothly.
        if (playerMovementState == PlayerMovementState.Stop)
        {
            InitializePhysicsProperties();
        }
    }

    /// <summary>
    /// Get player direction on X-axis as a vector2.
    /// </summary>
    public Vector2 GetPlayerDirection()
    {
        return isFacingRight ? Vector2.right : Vector2.left;
    }

    /// <summary>
    /// Flip the character about the x-axis through toggling the scale value of the transform property.
    /// </summary>
    private void FlipCharacter()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1.0f, transform.localScale.y, 1.0f);
    }

    /// <summary>
    /// Return gravity scale and linear drag to their initial state.
    /// </summary>
    private void InitializePhysicsProperties()
    {
        myBody.gravityScale = initialGravity;
        myBody.linearDamping = initialLinearDrag;
    }

    private void HandlePlayerAnimations()
    {
        if (!IsPlayerAnimated && !canMove) return;

        if(playerGrounded)
        {
            playerAnimator.SetBool("Move", Mathf.Abs(inputManager.GetHorizontalInput()) > 0.05f);

            playerAnimator.SetBool("Jump", canJump);
            playerAnimator.SetBool("Fall", false);
            isFalling = false;
        }
        else
        {
            playerAnimator.SetBool("Move", false);

            if (dash.PlayJumpAnimation)
            {
                playerAnimator.SetBool("Jump", true);
                return;
            }

            // Control jump animations.
            if (myBody.linearVelocity.y < 0.0f) // Falling state.
            {
                isFalling = true;
            }

            if (isFalling)
            {
                playerAnimator.SetBool("Jump", false);
                playerAnimator.SetBool("Fall", true);
            }

            // Play jump animation.
            if (IsPlayerAnimated)
            {
                playerAnimator.SetBool("Jump", 
                    (!isFalling && myBody.linearVelocity.y > 0.0f));
            }
        }
    }

    private void PrepareJumpMechanics()
    {
        if (inputManager.JumpInputPressed && playerGrounded)
        {
            canJump = true;
        }
    }

    private void HandleDifferentJumpMechanics()
    {
        if (canJump)
        {
            Jump();
        }

        if (!playerGrounded)
        {
            // No linear drag is needed when player in air.
            myBody.linearDamping = initialLinearDrag * 0.0f;

            myBody.gravityScale = initialGravity * gravityMultiplier;
        }
    }

    private void Jump()
    {
        canJump = false;

        myBody.linearVelocity = new Vector2(myBody.linearVelocity.x, 0.0f);

        // Perform the jump.
        myBody.AddForce(jumpForce, ForceMode2D.Impulse);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if(playerGrounded)
        {
            Gizmos.color = new Color(0.0f, 1.0f, 0.0f, 0.6f);
        }
        else
        {
            Gizmos.color = new Color(1.0f, 1.0f, 0.0f, 0.6f);
        }

        Gizmos.DrawLine(transform.TransformPoint(groundCheckStartPos) + groundCheckRaycastOffset,
            transform.TransformPoint(groundCheckStartPos) + groundCheckRaycastOffset + Vector3.down * raycastGroundChackLineLength);
        Gizmos.DrawLine(transform.TransformPoint(groundCheckStartPos) - groundCheckRaycastOffset,
            transform.TransformPoint(groundCheckStartPos) - groundCheckRaycastOffset + Vector3.down * raycastGroundChackLineLength);
    }
#endif
}
