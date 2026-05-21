using UnityEngine;

public class ScreenshotTaker : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            // Додали Time.frameCount, щоб назви завжди були унікальними
            string fileName = "Screenshot_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + Time.frameCount + ".png";
            ScreenCapture.CaptureScreenshot(fileName);
            Debug.Log("Скріншот збережено: " + fileName);
        }
    }
}