using UnityEngine;
using UnityEngine.Animations.Rigging;

public class EquipWeapon : MonoBehaviour
{
    [Header("Ray Settings")]
    [SerializeField][Range(0.0f, 2.0f)] private float rayLength;
    [SerializeField] private Vector3 rayOffset;
    [SerializeField] private LayerMask weaponMask;
    private RaycastHit topRayHitInfo;
    private RaycastHit bottomRayHitInfo;

    private Weapon currentWeapon;

    [SerializeField] private Transform equipPos;
    [SerializeField] private Transform aimingPos;

    private Animator playerAnimator;

    private bool isAiming;

    [Header("Right Hand Target")]
    [SerializeField] private TwoBoneIKConstraint rightHandIK;
    [SerializeField] private Transform rightHandTarget;

    [Header("Left Hand Target")]
    [SerializeField] private TwoBoneIKConstraint leftHandIK;
    [SerializeField] private Transform leftHandTarget;

    [SerializeField] private Transform IKRightHandPos;
    [SerializeField] private Transform IKLeftHandPos;

    private bool IsEquiped;

    void Start()
    {
        playerAnimator = GetComponent<Animator>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            Equip();
        }
         
        if(Input.GetKeyDown(KeyCode.Q))
        {
            UnEquip();
        }

        if(Input.GetButton("Fire1"))
        {
            playerAnimator.SetBool("RevolverAim", true);

            isAiming = true;
        }
        else if (Input.GetButtonUp("Fire1"))
        {
            playerAnimator.SetBool("RevolverAim", false);

            isAiming = false;
        }

        if (Input.GetButtonDown("Fire2"))
        {
            playerAnimator.SetBool("AssaultAim", true);

            isAiming = true;
        }
        else if (Input.GetButtonUp("Fire2"))
        {
            playerAnimator.SetBool("AssaultAim", false);

            isAiming = false;
        }

        if(currentWeapon)
        {
            if(!isAiming)
            {
                currentWeapon.transform.parent = equipPos.transform;
                currentWeapon.transform.position = equipPos.position;
                currentWeapon.transform.rotation = equipPos.rotation;

                leftHandIK.weight = 0f;
            }
            else
            {
                //currentWeapon.transform.parent = aimingPos.transform;
                //currentWeapon.transform.position = aimingPos.position;
                //currentWeapon.transform.rotation = aimingPos.rotation;

                leftHandIK.weight = 1f;
                leftHandTarget.position = IKLeftHandPos.position;
                leftHandTarget.rotation = IKLeftHandPos.rotation;
            }

            //rightHandIK.weight = 1f;
            //rightHandTarget.position = IKRightHandPos.position;
            //rightHandTarget.rotation = IKRightHandPos.rotation;
        }

    }

    private void RaycastsHandler()
    {
        Ray topRay = new Ray(transform.position + rayOffset, transform.forward);
        Ray bottomRay = new Ray(transform.position + Vector3.up * 0.175f, transform.forward);

        Debug.DrawRay(transform.position + rayOffset, transform.forward * rayLength, Color.red);
        Debug.DrawRay(transform.position + Vector3.up * 0.175f, transform.forward * rayLength, Color.green);

        Physics.Raycast(topRay, out topRayHitInfo, rayLength, weaponMask);
        Physics.Raycast(bottomRay, out bottomRayHitInfo, rayLength, weaponMask);
    }

    private void Equip()
    {
        RaycastsHandler();

        if (topRayHitInfo.collider != null)
        {
            currentWeapon = topRayHitInfo.transform.gameObject.GetComponent<Weapon>();
        }

        if (bottomRayHitInfo.collider)
        {
            currentWeapon = bottomRayHitInfo.transform.gameObject.GetComponent<Weapon>();
        }

        if (!currentWeapon) return;

        // Stop weapon rotation.
        currentWeapon.IsRotating = false;

        currentWeapon.ChangeWeaponBehaviour();

        IsEquiped = true;
    }

    private void UnEquip()
    {
        if (IsEquiped)
        {
            rightHandIK.weight = 0.0f;

            if (IKLeftHandPos)
            {
                leftHandIK.weight = 0.0f;
            }

            IsEquiped = false;
            currentWeapon.transform.parent = null;

            currentWeapon.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            currentWeapon.GetComponent<Rigidbody>().isKinematic = false;

            currentWeapon = null;
        }
    }
}
