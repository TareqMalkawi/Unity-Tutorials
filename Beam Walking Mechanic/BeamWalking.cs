using System.Collections.Generic;
using UnityEngine;

public class BeamWalking : MonoBehaviour
{
    [SerializeField] private float rayDistance = 2f;
    [SerializeField] private LayerMask beamMask;
    public bool OnBeam { get; private set; }

    // Reference to the CharacterController component.
    private CharacterController characterController;

    private Quaternion currentBeamRotation;

    private Animator animator;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(OnBeam)
        {
            animator.SetBool("isSneaking", true);

            // Moving forward.
            if (Input.GetAxis("Vertical") > 0.05f)
            {
                transform.rotation = currentBeamRotation;

                animator.SetFloat("Sneaking", Input.GetAxis("Vertical"), 0.2f, Time.deltaTime);

                characterController.Move(Input.GetAxis("Vertical") * Time.deltaTime * transform.forward);
            }
            // Moving backward.
            else if (Input.GetAxis("Vertical") < -0.05f)
            {
                transform.rotation = currentBeamRotation;

                // Rotate the character by 180 degrees.
                transform.rotation = Quaternion.AngleAxis(180f, Vector3.up) * transform.rotation;

                animator.SetFloat("Sneaking", Mathf.Abs(Input.GetAxis("Vertical")), 0.2f, Time.deltaTime);

                characterController.Move(Quaternion.AngleAxis(180f, Vector3.up) * transform.forward *
                    Input.GetAxis("Vertical") * Time.deltaTime);
            }
            else
            {
                animator.SetFloat("Sneaking", 0, 0f, Time.deltaTime);
            }
        }
        else
        {
            animator.SetFloat("Sneaking", 0, 0.1f, Time.deltaTime);
            animator.SetBool("isSneaking", false);
        }
    }

    private void FixedUpdate()
    {
        if(PhysicsUtils.ThreeRaycasts(transform.position + transform.forward * 0.3f, -transform.up, 0.5f, transform,
            out List<RaycastHit> hits, rayDistance, beamMask, true))
        {
            foreach(var hit in hits)
            {
                if(hit.collider != null)
                {
                    currentBeamRotation = hit.transform.rotation;
                }
            }

            OnBeam = true;
        }
        else
        {
            OnBeam = false;
        }
    }
}
