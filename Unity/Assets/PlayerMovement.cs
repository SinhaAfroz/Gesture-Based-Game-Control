using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // Adjust speed
    public float rotationSpeed = 10f; // Smooth rotation speed
    public float tiltAngle = 45f; // Tilt by 45 degrees
    public float tiltReturnSpeed = 5f; // Speed of returning to straight position

    private Rigidbody rb;
    private float currentTilt = 0f; // Current tilt of the capsule

    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Get Rigidbody
    }

    void Update()
    {
        float moveX = Input.GetAxis("Horizontal"); // A/D or Left/Right Arrow
        float moveZ = Input.GetAxis("Vertical"); // W/S or Up/Down Arrow

        Vector3 moveDirection = new Vector3(moveX, 0, moveZ).normalized;

        if (moveDirection.magnitude >= 0.1f)
        {
            // Rotate the capsule towards the movement direction (smooth)
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            // Apply tilt by adjusting the local rotation on the X-axis
            currentTilt = Mathf.Lerp(currentTilt, tiltAngle, Time.deltaTime * tiltReturnSpeed);
        }
        else
        {
            // Gradually straighten the capsule when it's not moving
            currentTilt = Mathf.Lerp(currentTilt, 0f, Time.deltaTime * tiltReturnSpeed);
        }

        // Apply the tilt to the capsule
        transform.rotation = Quaternion.Euler(currentTilt, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

        rb.MovePosition(transform.position + moveDirection * moveSpeed * Time.deltaTime);
    }
}
