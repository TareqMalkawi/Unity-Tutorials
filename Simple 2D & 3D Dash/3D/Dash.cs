using UnityEngine;

public class Dash : MonoBehaviour
{
    // References.
    private InputManager inputManager;
    private CharacterController characterController;
    private PlayerController playerController;
    private Camera mainCamera;

    private bool isCurrentlyDashing;
    private bool attemptToDash;

    [Header("Settings")]
    [SerializeField] private float maxDashTime;
    private float dashTime;

    [SerializeField] private float dashSpeed;

    [SerializeField] private float maxDashCoolDown;
    private float dashCoolDown;

    [Header("Effects")]

    [SerializeField] private bool enableDashAfterEffect;

    [SerializeField] private float dashEffectDistanceBetweenInstances;

    // Reference to dash effect object in code.
    private GameObject dashEffectRef;

    // Store the last instance pos.
    private Vector3 lastInstancePos;

    [SerializeField] private float cameraDashFOV;
    [SerializeField] private float cameraSpeedFOV;
    private float cameraDefaultFOV;
    private float cameraCurrentFOV;

    public PoolItem DashEffectItem;
    private Pool pool;

    void Start()
    {
        inputManager = GetComponent<InputManager>();
        characterController = GetComponent<CharacterController>();
        playerController = GetComponent<PlayerController>();
        
        pool = GetComponent<Pool>();
        if(pool)
        {
            pool.items.Add(DashEffectItem);
        }

        dashCoolDown = maxDashCoolDown;

        mainCamera = Camera.main;
        if (mainCamera)
        {
            cameraDefaultFOV = mainCamera.fieldOfView;
            cameraCurrentFOV = cameraDefaultFOV;
        }
    }

    private void Update()
    {
        HandleCameraFOV();
    }

    void FixedUpdate()
    {
        DashAbility();
    }

    private void HandleCameraFOV()
    {
        if (isCurrentlyDashing)
        {
            cameraCurrentFOV = Mathf.Lerp(cameraCurrentFOV, cameraDashFOV,
                cameraSpeedFOV * Time.deltaTime);
        }
        else
        {
            cameraCurrentFOV = Mathf.Lerp(cameraCurrentFOV, cameraDefaultFOV,
                cameraSpeedFOV * Time.deltaTime);
        }

        if (mainCamera.fieldOfView != cameraCurrentFOV)
        {
            mainCamera.fieldOfView = cameraCurrentFOV;
        }
    }

    private void GetDashEffectFromPool()
    {
        dashEffectRef = Pool.instance.Get(DashEffectItem.name);
        dashEffectRef.transform.position = transform.position;

        lastInstancePos = transform.position;
    }

    private void DashEffect()
    {
        if (enableDashAfterEffect)
        {
            if ((Mathf.Abs(transform.position.x - lastInstancePos.x) >
                   dashEffectDistanceBetweenInstances ||

                   Mathf.Abs(transform.position.z - lastInstancePos.z) >
                   dashEffectDistanceBetweenInstances))
            {
                GetDashEffectFromPool();
            }
        }
    }

    private void DashAbility()
    {
        if (inputManager.DashInputHold)
            attemptToDash = true;
        else
            attemptToDash = false;

        if (attemptToDash)
        {
            dashCoolDown += Time.fixedDeltaTime;
        }

        if (inputManager.DashInputHold && dashCoolDown > maxDashCoolDown)
        {
            isCurrentlyDashing = true;
        }

        if (isCurrentlyDashing)
        {
            dashTime += Time.fixedDeltaTime;
            playerController.CanMove = false;

            if (playerController.IsGrounded())
            {
                characterController.SimpleMove(dashSpeed * Time.fixedDeltaTime *
                        playerController.GetMovementDirection());
            }

            DashEffect();

            if (dashTime > maxDashTime)
            {
                dashTime = 0.0f;
                dashCoolDown = 0.0f;
                isCurrentlyDashing = false;
                playerController.CanMove = true;
            }
        }
    }
}