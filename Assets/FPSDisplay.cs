using UnityEngine;
using TMPro;

public class FPSDisplay : MonoBehaviour
{
    public TextMeshProUGUI fpsText; // Reference to a TextMeshProUGUI component to display FPS
    public float updateInterval = 0.5f; // Time interval for FPS updates
    public float lowestFpsWindow = 3f; // Window for tracking lowest FPS

    private float timeSinceLastUpdate = 0f;
    private int framesSinceLastUpdate = 0;
    private float fps = 0f;

    private float highestFPS = 0f;
    private float lowestFPS = Mathf.Infinity;
    private float timeSinceLowestReset = 0f; // Timer for resetting lowest FPS

    private void Update()
    {
        timeSinceLastUpdate += Time.deltaTime;
        timeSinceLowestReset += Time.deltaTime;
        framesSinceLastUpdate++;

        // Update FPS display at the specified interval
        if (timeSinceLastUpdate >= updateInterval)
        {
            // Calculate current FPS
            fps = framesSinceLastUpdate / timeSinceLastUpdate;

            // Update highest FPS
            if (fps > highestFPS) highestFPS = fps;

            // Update lowest FPS within the rolling 3-second window
            if (fps < lowestFPS) lowestFPS = fps;

            // Display FPS information
            fpsText.text = $"FPS: {fps:F1} (High: {highestFPS:F1}, Low: {lowestFPS:F1})";

            // Reset counters for current FPS calculation
            timeSinceLastUpdate = 0f;
            framesSinceLastUpdate = 0;
        }

        // Reset the lowest FPS every 3 seconds
        if (timeSinceLowestReset >= lowestFpsWindow)
        {
            lowestFPS = Mathf.Infinity;
            timeSinceLowestReset = 0f;
        }
    }
}
