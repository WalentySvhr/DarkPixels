using UnityEngine;
using System.IO; // Додаємо для роботи з папками та файлами

public class ScreenshotTaker : MonoBehaviour
{
    void Update()
    {
        // Краще використовувати F11 або іншу клавішу, бо F12 часто перехоплює Steam чи Windows
        if (Input.GetKeyDown(KeyCode.F12))
        {
            TakeScreenshot();
        }
    }

    private void TakeScreenshot()
    {
        // 1. Формуємо шлях до папки "Screenshots" поруч із папкою Assets
        string folderPath = Path.Combine(Application.dataPath, "../Screenshots");

        // 2. Якщо такої папки ще немає — створюємо її
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // 3. Формуємо ім'я файлу
        string fileName = "Screenshot_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + Time.frameCount + ".png";

        // 4. Об'єднуємо папку та ім'я файлу в повний шлях
        string fullPath = Path.Combine(folderPath, fileName);

        // 5. Робимо скріншот
        ScreenCapture.CaptureScreenshot(fullPath);

        // 6. Виводимо ПОВНИЙ шлях у консоль
        Debug.Log($"<color=magenta>[Screenshot]</color> Успішно збережено! Шукайте тут: \n{fullPath}");
    }
}