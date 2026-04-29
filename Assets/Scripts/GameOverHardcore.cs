using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverHardcore : MonoBehaviour
{
    public void RestartFromLastSave()
    {
        // 1. Повертаємо час у норму (якщо пауза)
        Time.timeScale = 1f;

        // 2. Кажемо SaveManager підготуватися до завантаження (як у Головному меню!)
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.PrepareLoad();
        }
        else
        {
            Debug.LogWarning("SaveManager не знайдено на сцені!");
        }

        // 3. Перезавантажуємо сцену. Тепер SaveManager чесно все завантажить!
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}