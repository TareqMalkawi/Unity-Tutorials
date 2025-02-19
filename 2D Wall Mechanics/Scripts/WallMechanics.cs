using UnityEngine;

public class WallMechanics : MonoBehaviour
{
    [Header("Raycast Properties")]
    [SerializeField] private LayerMask wallCheckLayer;
    [SerializeField] private float wallRayLength;
    [SerializeField] private float wallRayYOffset;
    [SerializeField] private float wallLedgeRayYOffset;
    [SerializeField] private float maxWallLedgeHeightExtendedValue;

    // Store the required hit information.
    private RaycastHit2D wallCheckHitInfo;
    private RaycastHit2D wallLedgeHitInfo;

    [Header("Wall Sliding Properties")]
    [SerializeField] private float wallSlidingSpeed;

    public bool IsWallSliding { get; private set; }

    [Header("Pop of Wall Properties")]
    [SerializeField] private bool allowWallPoping;

    // Instant force added to the body in the oppisite direction.
    [SerializeField] private float popOfWallInstantForce;

    // Determine if player has just poped of wall.
    private bool popedOfWall;

    [Header("Wall Jumping Properties")]
    [SerializeField] private bool allowWallJumping;

    // Wall jump speed on both axis.
    [SerializeField] private Vector2 wallJumpVelocity;

    // Determine the direction of the wall jump.
    private float wallJumpingDirection;

    public bool CanWallJump { get; private set; }

    [Header("Wall Climbing Properties")]
    [SerializeField] private bool allowWallClimbing;

    // Wall climbing speed.
    [SerializeField] private float wallClimbingSpeed;
    [SerializeField] private float wallClimbingSmoothing;

    public bool IsWallClimbing { get; private set; }

    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    
    [SerializeField] private InputManager inputManager;

    private Animator animator;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();

        wallJumpingDirection = 1;
    }

    void Update()
    {
        WallMechanicsHandler();

        WallClimbing();
    }

    /// <summary>
    /// Detect any walls in front of the player.
    /// </summary>
    public RaycastHit2D WallCheck()
    {
        Physics2D.queriesStartInColliders = false;  // Prevent the ray from hitting the player.

        wallCheckHitInfo = Physics2D.Raycast(new Vector3(transform.position.x, transform.position.y + wallRayYOffset, 1.0f),
            playerMovement.GetPlayerDirection() == Vector2.right ? Vector2.right : -Vector2.right, wallRayLength, wallCheckLayer);

        return wallCheckHitInfo;
    }

    private void WallMechanicsHandler()
    {
        // If there is a wall in front, but player is grounded, then there is no need to do any thing.
        if (WallCheck().collider == null || playerMovement.PlayerGrounded) 
        {
            IsWallSliding = false;

            if (playerMovement.PlayerGrounded)
                CanWallJump = false;

            popedOfWall = false;

            return;
        }

        // Perform wall sliding while the player velocity is slowing down and the player
        // is pressing the horizontal input in the direction of the wall.
        if (playerMovement.MyBody.linearVelocity.y < 0.0f)
        {
            if ((playerMovement.GetPlayerDirection() == Vector2.right && inputManager.GetHorizontalInput() > 0.05f) ||
                   (playerMovement.GetPlayerDirection() == Vector2.left && inputManager.GetHorizontalInput() < -0.05f))
            {
                IsWallSliding = true;
            }
            else
            {
                IsWallSliding = false;
            }

            if (IsWallSliding)
            {
                playerMovement.MyBody.linearVelocity = new Vector2(playerMovement.MyBody.linearVelocity.x, 
                    Mathf.Clamp(playerMovement.MyBody.linearVelocity.y, -wallSlidingSpeed * Time.deltaTime
                    , float.MaxValue));

                // Set the jumping direction while sliding, so player jumps correctly.
                wallJumpingDirection = -transform.localScale.x;

                // Player jumped while sliding. Then stop sliding and perform the jump.
                if (inputManager.JumpInputPressed && allowWallJumping)
                {
                    CanWallJump = true;
                    IsWallSliding = false;
                }
                else
                {
                    CanWallJump = false;
                    popedOfWall = true;
                }
            }

            // Wall jump.
            if (CanWallJump)
            {
                if (!inputManager.JumpInputPressed)
                    return;

                WallJumpHandler();
            }

            // Pop of wall.  
            if (!allowWallPoping) return;

            if (!inputManager.JumpInputPressed && !IsWallSliding && !popedOfWall)
            {
                PopOfWallHandler();
            }
        }
    }

    /// <summary>
    /// Perform the wall jump by updating the velocity in the oppsite direction of the player input. And makes sure
    /// to flip the character accordingly.
    /// </summary>
    private void WallJumpHandler()
    {
        playerMovement.MyBody.linearVelocity = new Vector2(-inputManager.GetHorizontalInput() * wallJumpVelocity.x,
            wallJumpVelocity.y);

        if (transform.localScale.x != wallJumpingDirection)
        {
            playerMovement.FlipCharacter();
        }
    }

    /// <summary>
    /// Perform the popOfWall mechanic by adding force in the direction calculated by vector reflection.
    /// </summary>
    private void PopOfWallHandler()
    {
        popedOfWall = true;

        var d = new Vector2(playerMovement.GetPlayerDirection().x, playerMovement.GetPlayerDirection().y + 1.0f);
        var popForceDir = Vector2.Reflect(d, WallCheck().normal);

        playerMovement.MyBody.linearVelocity = Vector2.zero;
        playerMovement.MyBody.AddForce(popOfWallInstantForce * Time.deltaTime * popForceDir, ForceMode2D.Impulse);

        popedOfWall = false;
    }

    private void WallClimbing()
    {
        if (!allowWallClimbing) return;

        if (WallCheck().collider == null)
        {
            IsWallClimbing = false;
            
            if (animator.GetBool("Climb") == true)
            {
                animator.SetBool("Climb", false);
                animator.SetBool("ClimbStart", false);
            }

            inputManager.WallClimpingInputHold = false;
            return;
        }

        if (inputManager.WallClimpingInputHold)
        {
            // Handle ledge automatically while player performing the climb.
            LedgeAuto();

            IsWallSliding = false;

            animator.SetBool("ClimbStart", true);

            //popedOfWall = true;
            IsWallClimbing = true;

            playerMovement.MyBody.gravityScale = playerMovement.initialGravity * 0.0f;

            // Move up.
            if (inputManager.GetVerticalInput() > 0.05f)
            {
                animator.SetBool("Climb", true);

                playerMovement.MyBody.linearVelocity = new Vector2(0.0f, wallClimbingSpeed * Ease(wallClimbingSmoothing, 
                    Time.fixedDeltaTime));
            }

            // Move down.
            else if (inputManager.GetVerticalInput() < -0.05f)
            {
                animator.SetBool("Climb", true);

                playerMovement.MyBody.linearVelocity = new Vector2(0.0f, -wallClimbingSpeed * Ease(wallClimbingSmoothing, 
                    Time.fixedDeltaTime));
            }

            // Stop.
            else
            {
                animator.SetBool("Climb", false);

                playerMovement.MyBody.linearVelocity = new Vector2(0.0f, 0.0f);
            }

            // When there is a sudden change in direction, climbing should stop.
            if ((inputManager.GetHorizontalInput() > 0.05f && playerMovement.GetPlayerDirection() == Vector2.left) ||
                (inputManager.GetHorizontalInput() < -0.05f && playerMovement.GetPlayerDirection() == Vector2.right))
            {
                animator.SetBool("ClimbStart", false);
                playerMovement.MyBody.gravityScale = playerMovement.initialGravity * playerMovement.gravityMultiplier;
                return;
            }
        }
        else
        {
            IsWallClimbing = false;

            animator.SetBool("Climb", false);
            animator.SetBool("ClimbStart", false);
        }
    }

    /// <summary>
    /// If you are interested, you can read more about exponential decay
    /// on the Wiki page https://en.wikipedia.org/wiki/Exponential_decay
    /// </summary>
    /// <param name="speed"> constant value (called lambda) </param>
    /// <param name="deltaTime"> in Unity we can use Time.Deltatime or Time.time </param>
    protected float Ease(float speed, float deltaTime)
    {
        return 1 - Mathf.Exp(-speed * deltaTime);
    }

    private void LedgeAuto()
    {
        wallLedgeHitInfo = Physics2D.Raycast(new Vector3(transform.position.x, transform.position.y + wallLedgeRayYOffset, 1.0f),
            playerMovement.GetPlayerDirection() == Vector2.right ? Vector2.right : -Vector2.right, wallRayLength, wallCheckLayer);

        if (WallCheck().collider && !wallLedgeHitInfo.collider)
        {
            float requiredHeight = wallLedgeRayYOffset + maxWallLedgeHeightExtendedValue;

            var hitObj = transform.InverseTransformPoint(WallCheck().collider.gameObject.transform.position);

            float width = hitObj.x / 2.0f;

            var target = playerMovement.GetPlayerDirection() == Vector2.right ? new Vector2(width, requiredHeight) :
                new Vector2(-width, requiredHeight);

            var endPos = (Vector2)transform.position + target;

            transform.position = endPos;

            return;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!playerMovement) return;

        Gizmos.color = new Color(0.0f, 1.0f, 1.0f, 1.0f);

        Gizmos.DrawLine(new Vector3(transform.position.x, transform.position.y + wallRayYOffset, 1.0f),
          ((Vector3)playerMovement.GetPlayerDirection() == Vector3.right ? Vector3.right : Vector3.left) * wallRayLength +
          new Vector3(transform.position.x, transform.position.y + wallRayYOffset, 1.0f));

        if (allowWallClimbing)
        {
            Debug.DrawRay(new Vector3(transform.position.x, transform.position.y + wallLedgeRayYOffset, 1.0f),
              playerMovement.GetPlayerDirection() == Vector2.right ? Vector2.right * wallRayLength : 
              -Vector2.right * wallRayLength, Color.magenta);
        }
    }
#endif
}