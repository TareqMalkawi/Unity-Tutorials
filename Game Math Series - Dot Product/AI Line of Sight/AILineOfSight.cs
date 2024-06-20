using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class AILineOfSight : MonoBehaviour
{
    [SerializeField] private Transform target;
    private Animator animator;
    private NavMeshAgent navMeshAgent;

    // How far the NBC has to stay from target when he suddenly stops
    // could be anything I simply initilaize it to the agent stopping distance.
    [SerializeField] private float stoppingDist;

    // Field of View (FOV), the NBC line of sight.
    // Remember that any value greter than 90.0 degrees means that the NBC can detect what behinds him.
    [SerializeField] private float cutOffAngle = 45.0f;

    // AI agent forward vector
    private Vector3 AIForward;

    private Vector3 direction;

    [SerializeField] private Text resultText;

    private void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
        stoppingDist = navMeshAgent.stoppingDistance;
    }

    private void FixedUpdate()
    {
        SimpleNBCBehaviour();
    }

    private void LateUpdate()
    {
        UpdateUIBasedOnDotProductResult();
    }

    private void SimpleNBCBehaviour()
    {
        if (IsTargetInFrontOfMe())
        {
            animator.SetBool("Run", true);
            navMeshAgent.SetDestination(target.position);

            if (navMeshAgent.remainingDistance < stoppingDist)
            {
                animator.SetBool("Run", false);
                animator.SetTrigger("Attack");
            }
        }
        else
        {
            animator.SetBool("Run", false);
        }
    }

    private float GetDotProductResult()
    {
        AIForward = transform.forward;

        Debug.DrawRay(transform.position, AIForward, Color.yellow);

        direction = target.position - transform.position;
        direction.Normalize();

        Debug.DrawRay(transform.position, direction, Color.green);

        return Vector3.Dot(AIForward, direction);
    }

    bool IsTargetInFrontOfMe()
    {
        float desiredAngle = Mathf.Acos(GetDotProductResult() / (AIForward.magnitude * direction.magnitude));

        desiredAngle *= Mathf.Rad2Deg;

        if (desiredAngle < cutOffAngle)
            return true;

        return false;
    }

    private void UpdateUIBasedOnDotProductResult() => resultText.text = "Dot Product result: " + Mathf.RoundToInt(GetDotProductResult());
}