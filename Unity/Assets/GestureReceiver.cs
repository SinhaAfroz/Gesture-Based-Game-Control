using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System;
using UnityEngine;
using System.Collections;

public class GestureReceiver : MonoBehaviour
{
    private UdpClient udpClient;
    public int port = 12345; // The port on which to receive data (same as Python sender)
    public GameObject player; // Reference to the player capsule
    private Queue<string> gestureQueue = new Queue<string>(); // Queue to hold received gestures
    public Rigidbody playerRigidbody;
    private Renderer playerRenderer;

    // Ground check parameters
    public float groundCheckDistance = 1.1f; // Distance from the player to check for ground
    public LayerMask groundLayer; // Specify which layers are considered the ground

    // Tilt parameters
    public float tiltAngle = 45; // Tilt by 45 degrees
    public float tiltReturnSpeed = 5f; // Speed of returning to straight position
    private float currentTilt = 0f; // Current tilt angle of the player

    // Movement Speed for gesture handling
    public float moveSpeed = 10f;  // Speed of movement
    public float rotationSpeed = 5f; // Speed of rotation towards movement direction

    private Vector3 originalScale;
    public float punchScaleMultiplier = 1.2f; // Scale multiplier for punch effect
    public float punchDuration = 0.2f; // Duration for scale change
    public AudioSource punchSound; // AudioSource for punch sound
    public Color punchColor = Color.red; // Color to change when punching
    public float punchColorDuration = 0.2f; // Duration for color change

    private Material playerMaterial; // Instance of the player's material
    private Color originalColor; // Store the original color of the player

    void Start()
    {
        udpClient = new UdpClient(port);
        udpClient.BeginReceive(ReceiveData, null); // Begin asynchronous listening for data
        Debug.Log("UDP listening started...");

        if (playerRigidbody == null)
        {
            Debug.LogError("Rigidbody is not attached to the player!");
        }

        playerRigidbody = player.GetComponent<Rigidbody>();

        playerRenderer = player.GetComponent<Renderer>();  // Get the player's renderer for color change
        originalScale = player.transform.localScale;  // Store the player's original scale

        // Create an instance of the material to avoid changing other objects with the same material
        playerMaterial = new Material(playerRenderer.material);
        playerRenderer.material = playerMaterial; // Assign the instance material to the player

        // Store the original color before any changes occur
        originalColor = playerMaterial.color;
    }

    // This method is called when UDP data is received
    void ReceiveData(IAsyncResult ar)
    {
        try
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
            byte[] receivedBytes = udpClient.EndReceive(ar, ref endPoint);
            string message = Encoding.UTF8.GetString(receivedBytes);

            //Debug.Log("Received Data: " + message); // Debugging received data

            // Add the gesture to the queue
            lock (gestureQueue)
            {
                gestureQueue.Enqueue(message);
            }

            // Begin listening for the next message
            udpClient.BeginReceive(ReceiveData, null); // Recursively continue listening for new data
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error receiving data: " + e.Message);
        }
    }

    void Update()
    {
        // Process gestures from the queue
        if (gestureQueue.Count > 0)
        {
            string gesture;
            lock (gestureQueue)
            {
                gesture = gestureQueue.Dequeue(); // Get the next gesture
            }

            HandleGesture(gesture);
        }

        // Smooth tilt return when no movement input or gesture is detected
        if (gestureQueue.Count == 0 && !Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
        {
            currentTilt = Mathf.Lerp(currentTilt, 0f, Time.deltaTime * tiltReturnSpeed);
            ApplyTilt();
        }

        // Check for keyboard input for left and right movement
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            MoveLeft();
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            MoveRight();
        }
    }

    void HandleGesture(string gesture)
    {
        // Define jump force
        float jumpForce = 10f;

        // Check for different gestures
        switch (gesture)
        {
            case "swipe_left":
                //Debug.Log("Swipe Left detected!");
                MoveLeft();
                break;

            case "swipe_right":
                //Debug.Log("Swipe Right detected!");
                MoveRight();
                break;

            case "fist":
                //Debug.Log("Fist detected! Performing punch action.");
                PerformPunch();
                break;

            case "open_palm":
                //Debug.Log("Open Hand detected! Performing jump action.");
                // Add upward force for jumping if on the ground
                if (IsGrounded())
                {
                    playerRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                    Debug.Log("Jump Force Applied! Velocity: " + playerRigidbody.velocity);  // Debug jump force
                }
                break;

            default:
                //Debug.Log("Unknown gesture received.");
                break;
        }
    }

    void MoveLeft()
    {
        Debug.Log("Moving Left");

        // Rotate the capsule to 45 degrees on the Y-axis
        Quaternion targetRotation = Quaternion.Euler(0, 45, 0); // 45 degrees on Y-axis
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        // Move the player left
        playerRigidbody.MovePosition(player.transform.position + player.transform.right * -moveSpeed * Time.deltaTime);
    }

    void MoveRight()
    {
        Debug.Log("Moving Right");

        // Rotate the capsule to -45 degrees on the Y-axis (for the opposite direction)
        Quaternion targetRotation = Quaternion.Euler(0, -45, 0); // -45 degrees on Y-axis
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        // Move the player right
        playerRigidbody.MovePosition(player.transform.position + player.transform.right * moveSpeed * Time.deltaTime);
    }

    void ApplyTilt()
    {
        // Tilt along the X-axis and keep the Y and Z axis unchanged
        transform.rotation = Quaternion.Euler(currentTilt, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
    }

    // Ground check function using Raycast
    bool IsGrounded()
    {
        //Debug.Log("Is Grounded");
        // Raycast down from the player's position to check if the ground is below
        return Physics.Raycast(player.transform.position, Vector3.down, groundCheckDistance, groundLayer);
    }

    void PerformPunch()
    {
        // Play the punch sound
        if (punchSound != null)
        {
            punchSound.Play();
        }

        // Apply scaling effect to simulate punch
        StartCoroutine(PunchEffect());

        // Apply color change to simulate punch impact
        StartCoroutine(PunchColorChange());
    }

    IEnumerator PunchEffect()
    {
        // Scale up the player temporarily
        player.transform.localScale = originalScale * punchScaleMultiplier;

        // Wait for the punch effect duration
        yield return new WaitForSeconds(punchDuration);

        // Return the player to its original scale
        player.transform.localScale = originalScale;
    }

    IEnumerator PunchColorChange()
    {
        // Change the color to the punch color
        playerMaterial.SetColor("_Color", punchColor);

        // Wait for the color change duration
        yield return new WaitForSeconds(punchColorDuration);

        // Restore the original color
        playerMaterial.SetColor("_Color", originalColor);
    }


    void OnApplicationQuit()
    {
        udpClient.Close(); // Close the UDP connection when the app quits
        Debug.Log("UDP listener stopped.");
    }
}
