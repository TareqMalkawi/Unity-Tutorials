using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float height;

    private Quaternion targetRotation;

    private float yRotation;
    private float xRotation;
    private float xRotationClamped;

    [SerializeField] private float xRotationMin;
    [SerializeField] private float xRotationMax;

    [SerializeField] private float xSensitivity;
    [SerializeField] private float ySensitivity;

    [SerializeField] private bool invertX;
    private int xInvertedValue;

    private Vector3 desiredPos;

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        xInvertedValue = invertX ? -1 : 1;
    }

    private void Update()
    {
        yRotation += Input.GetAxis("Mouse X") * ySensitivity;
        xRotation += Input.GetAxis("Mouse Y") * xSensitivity * xInvertedValue;
    }

    private void LateUpdate()
    {
        xRotationClamped = Mathf.Clamp(xRotation, xRotationMin, xRotationMax);
        targetRotation = Quaternion.Euler(xRotationClamped, yRotation, 0.0f);

        desiredPos = target.position - targetRotation * offset + Vector3.up * height;

        transform.SetPositionAndRotation(desiredPos, targetRotation);
    }

    public Quaternion YRotation => Quaternion.Euler(0.0f, yRotation, 0.0f);
}
