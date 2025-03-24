using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // Assign Capsule here
    public Vector3 offset = new Vector3(0, 1, -5); // Modify the offset to view tilt more clearly
    public float smoothingSpeed = 0.125f; // Control how smooth the camera follows the player

    void LateUpdate()
    {
        // Calculate desired position
        Vector3 desiredPosition = player.position + offset;

        // Smoothly move the camera towards the desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothingSpeed);

        // Update camera position
        transform.position = smoothedPosition;

        // Optional: Make the camera always look at the player
        transform.LookAt(player);
    }
}
