import cv2
import mediapipe as mp
import socket
import time

# Initialize MediaPipe hands
mp_hands = mp.solutions.hands
mp_drawing = mp.solutions.drawing_utils

# Initialize UDP Socket
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
server_address = ('127.0.0.1', 12345)  # Unity's IP and port


# Initialize webcam
cap = cv2.VideoCapture(0)

# Set up MediaPipe Hands with required parameters
with mp_hands.Hands(min_detection_confidence=0.5, min_tracking_confidence=0.5) as hands:
    last_position = None  # Global variable to track the last position

    # Function to check movement (left or right) based on hand landmarks
    def calculate_hand_center(hand_landmarks):
        wrist = hand_landmarks.landmark[mp_hands.HandLandmark.WRIST]
        thumb_tip = hand_landmarks.landmark[mp_hands.HandLandmark.THUMB_TIP]
        index_tip = hand_landmarks.landmark[mp_hands.HandLandmark.INDEX_FINGER_TIP]
        middle_tip = hand_landmarks.landmark[mp_hands.HandLandmark.MIDDLE_FINGER_TIP]
        ring_tip = hand_landmarks.landmark[mp_hands.HandLandmark.RING_FINGER_TIP]
        pinky_tip = hand_landmarks.landmark[mp_hands.HandLandmark.PINKY_TIP]

        # Calculate the center by averaging the x, y coordinates of key landmarks
        center_x = (wrist.x + thumb_tip.x + index_tip.x + middle_tip.x + ring_tip.x + pinky_tip.x) / 6
        center_y = (wrist.y + thumb_tip.y + index_tip.y + middle_tip.y + ring_tip.y + pinky_tip.y) / 6
        return (center_x, center_y)

    # Function to check movement (left, right, up, down)
    def detect_hand_movement(current_position, previous_position):
        if previous_position is not None:
            # Compare the current center position with the previous one
            movement_x = current_position[0] - previous_position[0]
            movement_y = current_position[1] - previous_position[1]

            # Detect movement in the X axis (left/right)
            if movement_x > 0.05:
                # print("Move Right")
                sock.sendto("swipe_right".encode('utf-8'), server_address)  # Send the move right to Unity
            elif movement_x < -0.05:
                # print("Move Left")
                sock.sendto("swipe_left".encode('utf-8'), server_address)  # Send the move left to Unity

        return current_position

    # Function to detect if a fist is made
    def is_fist(hand_landmarks):
        """Returns True if all fingers are curled (closed fist)"""
        fingers = [mp_hands.HandLandmark.INDEX_FINGER_TIP,
                   mp_hands.HandLandmark.MIDDLE_FINGER_TIP,
                   mp_hands.HandLandmark.RING_FINGER_TIP,
                   mp_hands.HandLandmark.PINKY_TIP]

        curled_fingers = 0
        for finger in fingers:
            fingertip = hand_landmarks.landmark[finger]
            knuckle = hand_landmarks.landmark[finger - 2]  # Compare with the PIP joint (middle knuckle)

            if fingertip.y > knuckle.y:  # If the tip is below the knuckle (closed)
                curled_fingers += 1

        return curled_fingers == 4  # True if all 4 fingers are curled

    # Function to detect if an open palm is made
    def is_open_palm(hand_landmarks):
        """Returns True if all fingers are extended (open palm)"""
        fingers = [mp_hands.HandLandmark.INDEX_FINGER_TIP,
                   mp_hands.HandLandmark.MIDDLE_FINGER_TIP,
                   mp_hands.HandLandmark.RING_FINGER_TIP,
                   mp_hands.HandLandmark.PINKY_TIP]

        extended_fingers = 0
        for finger in fingers:
            fingertip = hand_landmarks.landmark[finger]
            knuckle = hand_landmarks.landmark[finger - 2]  # Compare with PIP joint (middle knuckle)

            if fingertip.y < knuckle.y:  # If the tip is above the knuckle (extended)
                extended_fingers += 1

        return extended_fingers == 4  # True if all fingers are extended


    while cap.isOpened():
        ret, frame = cap.read()
        if not ret:
            continue

        # Flip the frame horizontally for a more natural view
        frame = cv2.flip(frame, 1)

        # Convert the frame to RGB for MediaPipe
        rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)

        # Process the frame and get hand landmarks
        results = hands.process(rgb_frame)

        # Draw landmarks if hands are detected
        if results.multi_hand_landmarks:
            for hand_landmarks in results.multi_hand_landmarks:
                mp_drawing.draw_landmarks(frame, hand_landmarks, mp_hands.HAND_CONNECTIONS)

                # Check for fist and open palm gestures
                gesture = None
                if is_fist(hand_landmarks):
                    gesture = "fist"
                elif is_open_palm(hand_landmarks):
                    gesture = "open_palm"

                # Calculate current hand center
                current_position = calculate_hand_center(hand_landmarks)

                # Check for swipe gestures (left or right) by checking hand movement
                last_position = detect_hand_movement(current_position, last_position)

                # Send the detected gesture to Unity
                if gesture:
                    sock.sendto(gesture.encode(), server_address)

        # Show the frame with the landmarks
        cv2.imshow("Hand Gesture Detection", frame)


        # Break the loop if 'q' is pressed
        if cv2.waitKey(1) & 0xFF == ord('q'):
            break

    cap.release()
    cv2.destroyAllWindows()
