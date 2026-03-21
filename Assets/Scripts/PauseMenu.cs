using UnityEngine;
using UnityEngine.SceneManagement; // Для повернення в головне меню

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI; // Панель меню, яку ми створимо далі
    private bool isPaused = false;

    // Метод для кнопки на ігровому екрані
    public void ToggleMenu()
    {
        if (isPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false); // Ховаємо меню
        Time.timeScale = 1f;          // Запускаємо час
        isPaused = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);  // Показуємо меню
        Time.timeScale = 0f;          // Зупиняємо час (фізика і рух стануть на паузу)
        isPaused = true;
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // ОБОВ'ЯЗКОВО повертаємо час у норму перед виходом
        SceneManager.LoadScene("MainMenu");
    }
}