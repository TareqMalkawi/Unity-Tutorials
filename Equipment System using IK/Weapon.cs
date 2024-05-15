using UnityEngine;

public class Weapon : MonoBehaviour
{
    private Rigidbody weaponBody;

    [SerializeField] private float rotationSpeed;

    public bool IsRotating { get; set; }

    void Start()
    {
        weaponBody = GetComponent<Rigidbody>();

        if (weaponBody)
        {
            weaponBody.isKinematic = true;
        }

        IsRotating = true;
    }

    void Update()
    {
        if (!IsRotating) return;

        transform.Rotate(Vector3.up * rotationSpeed * (1 - Mathf.Exp(-rotationSpeed * Time.deltaTime)));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            if (weaponBody)
                weaponBody.constraints = RigidbodyConstraints.FreezePosition;

            IsRotating = true;
        }
    }

    public void ChangeWeaponBehaviour()
    {
        if (weaponBody)
        {
            weaponBody.isKinematic = true;
            weaponBody.constraints = RigidbodyConstraints.None;
        }
    }
}
