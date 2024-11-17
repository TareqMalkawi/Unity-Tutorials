using UnityEngine;

public class Dash : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private InputManager inputManager;

    public enum DashType
    {
        Horizontal,
        Directional
    }

    [Header("Settings")]

    [SerializeField] private DashType dashType;

    // Dash speed determined by user.
    [SerializeField] private float dashSpeed;

    // The time period the dash will takes effect within determined by user.
    [SerializeField] public float maxDashTime;
    private float dashTime;

    // Constarin the dash to happens only once per X seconds.
    // Could be 0 seconds and the dash will happen as soon as the button is pressed.
    [SerializeField] private float maxDashCoolDown;
    private float dashCoolDown;

    // if player dashing this frame.
    private bool isCurrentlyDashing;

    private Vector2 dashDir;

    // Determine if the player is allowed to dash in air for only one time.
    [SerializeField] private bool allowDashInAir;

    // if player can dash in air.
    private bool canDashInAir;

    // Keep track of dash in air. Dash in air should only happens once.
    private int dashCounterInAir = 1;

    [Header("Effect")]

    // Control the dash effect state.
    [SerializeField] private bool enableDashAfterEffect;
    private bool disableDashEffect;

    public bool PlayJumpAnimation { get; set; }

    public PoolItem DashEffectItem;

    // Store the last image pos.
    private float lastImageXPos;
    private float lastImageYPos;

    // Distance between the after-image effect.
    [SerializeField] private float dashEffectDistanceBetweenImages;

    // Reference to dash effect object in code.
    private GameObject dashEffectRef;

    private void OnEnable()
    {
        Pool.instance.items.Add(DashEffectItem);
    }

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        inputManager = GetComponent<InputManager>();

        dashTime = maxDashTime;
        dashCoolDown = maxDashCoolDown;
    }

    private void Update()
    {
        if (inputManager.DashInputPressed)
        {
            if (!playerMovement.PlayerGrounded && dashCounterInAir == 1 && allowDashInAir)
                canDashInAir = true;
        }

        // To disable dash effect.
        if (dashCounterInAir == 2 && inputManager.DashInputPressed)
        {
            disableDashEffect = true;
        }
    }

    void FixedUpdate()
    {
        DashAbility();
    }

    /// <summary>
    /// Get an instance of dash effect prefab from the pool. And update its transform properly.
    /// </summary>
    private void GetDashEffectFromPool()
    {
        dashEffectRef = Pool.instance.Get(DashEffectItem.name);
        dashEffectRef.transform.position = transform.position;
        lastImageXPos = transform.position.x;
        lastImageYPos = transform.position.y;
    }

    /// <summary>
    /// Handle the dash after-image effect. 
    /// Position the prefabs correctly by separating them by some distance on both axis.
    /// </summary>
    private void DashEffect()
    {
        if (enableDashAfterEffect)
        {
            if (dashCounterInAir <= 2 && 
                (Mathf.Abs(transform.position.x - lastImageXPos) > dashEffectDistanceBetweenImages || 
                Mathf.Abs(transform.position.y - lastImageYPos) > dashEffectDistanceBetweenImages))
            {
                if (disableDashEffect)
                {
                    dashCounterInAir = 3;
                    return;
                }

                GetDashEffectFromPool();
            }
        }
    }

    private void DashAbility()
    {
        if (playerMovement.PlayerGrounded)
        {
            // Only one dash in air is allowed.
            dashCounterInAir = 1;

            // Make sure the effect stops after finish playing.
            disableDashEffect = false;

            // Determine if player can dash in air.
            canDashInAir = false;
        }
       
        dashCoolDown += Time.fixedDeltaTime;

        if (inputManager.DashInputHold && dashCoolDown > maxDashCoolDown)
        {
            isCurrentlyDashing = true;
        }

        if (isCurrentlyDashing)
        {
            dashTime += Time.fixedDeltaTime;
            playerMovement.CanMove = false;

            if (playerMovement.PlayerGrounded)
            {
                if (dashType == DashType.Horizontal)
                {
                    playerMovement.MyBody.linearVelocity =
                        new Vector2(playerMovement.GetPlayerDirection().x * dashSpeed, 0.0f);
                }
                else if (Mathf.Abs(inputManager.GetHorizontalInput()) != 0 ||
                    Mathf.Abs(inputManager.GetVerticalInput()) != 0)
                {
                    // Calculate the dash direction. In all 8 directions.
                    dashDir = Vector2.right * inputManager.GetHorizontalInput() + 
                        Vector2.up * inputManager.GetVerticalInput();
                    
                    dashDir.Normalize();

                    playerMovement.MyBody.linearVelocity = dashDir * dashSpeed;
                }
            }
            else if (canDashInAir)
            {
                canDashInAir = false;
                dashCounterInAir += 1;

                PlayJumpAnimation = true;

                if (dashType == DashType.Directional)
                {
                    dashDir = Vector2.right * inputManager.GetHorizontalInput() + 
                        Vector2.up * inputManager.GetVerticalInput();

                    dashDir.Normalize();

                    playerMovement.MyBody.linearVelocity = dashDir * dashSpeed;
                }
                else
                {
                    playerMovement.MyBody.linearVelocity = 
                        new Vector2(playerMovement.GetPlayerDirection().x * dashSpeed, 0.0f);
                }
            }

            DashEffect();

            // Dash will happen for a fixed amount of time determined by user.
            if (dashTime > maxDashTime)
            {
                PlayJumpAnimation = false;

                dashTime = 0.0f;
                dashCoolDown = 0.0f;
                isCurrentlyDashing = false;
                playerMovement.CanMove = true;

                dashEffectRef = null;
            }
        }
    }
}