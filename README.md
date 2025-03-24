# Gesture Recognition with Unity and MediaPipe

This project uses **MediaPipe** for hand gesture recognition and **OpenCV** for webcam input. It sends recognized gestures to **Unity** via **UDP** for interactive game actions.

### Requirements

- **Python 3.8**
- **OpenCV**
- **MediaPipe**
- **Unity 2022.3.58 LTS (Game Engine)**
- **Webcam (for input)**

### Project Structure

This repository contains two main folders:

- **Python Controller Folder**: 'GestureGameController' Contains the Python scripts for hand gesture tracking.
- **Unity Folder**: Contains the Unity project for gesture-based interaction.

### Python Setup

#### 1. Install Python 3.8

Make sure you have **Python 3.8** installed. You can download it from the [official Python website](https://www.python.org/downloads/release/python-380/).

#### 2. Install Required Libraries

Once Python is installed, create a virtual environment (optional but recommended), and install the necessary libraries:

  ```bash
  pip install opencv-python
 ```
```bash
 pip install mediapipe
```
#### 3. To run the Python code
 ```bash
 python hand_gesture_tracking.py 
```
Once the script is running, Unity will receive the gestures through the UDP channel, and the game will perform the following actions:

- Swipe Left/Right: Move the player character left or right.
- Fist: Change the player's color and trigger vibration.
- Open Palm: Make the player jump.








   
