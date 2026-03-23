using UnityEngine;
using UnityEngine.SceneManagement; // Потрібно для роботи зі сценами

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    private bool isPaused = false;

    public void ToggleMenu()
    {
        if (isPaused) Resume();
        else Pause();
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; // Повертаємо швидкість часу в норму
        isPaused = false;
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // Зупиняємо все у грі
        isPaused = true;
    }

    // --- НОВИЙ МЕТОД ДЛЯ КНОПКИ "RESTART" ---
    public void Restart()
    {
        Time.timeScale = 1f; // КРИТИЧНО ВАЖЛИВО: розморозити час перед перезапуском!
        // Завантажуємо поточну активну сцену за її назвою
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}