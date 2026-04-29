using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public SceneFader fader; // Перетягни сюди об'єкт FadePanel зі скриптом SceneFader
    private bool isPaused = false;

    public void ToggleMenu()
    {
        if (isPaused) Resume();
        else Pause();
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        // Використовуємо фейдер для перезапуску поточної сцени
        fader.FadeTo(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        // Використовуємо фейдер для повернення в меню
        // SceneFader сам переключить орієнтацію на Portrait, поки екран чорний
        fader.FadeTo("MainMenu");
    }
}