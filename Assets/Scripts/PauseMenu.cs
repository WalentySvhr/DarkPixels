using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic; // Необхідно для роботи зі списками List<>

public class PauseMenu : MonoBehaviour
{
    [Header("Панелі Інтерфейсу")]
    public GameObject pauseMenuUI;          // Твій головний PausePanel
    [Tooltip("Перетягни сюди StatisticsContainer")]
    public GameObject statisticsContainer;
    [Tooltip("Перетягни сюди QuestContainer")]
    public GameObject questContainer;

    [Tooltip("Перетягни сюди сам QuestContainer, на якому висить скрипт QuestListUI")]
    public QuestListUI questListManager;

    [Header("Посилання")]
    public SceneFader fader;                // Об'єкт FadePanel зі скриптом SceneFader
    private bool isPaused = false;

    void Start()
    {
        // При старті гри про всяк випадок ховаємо квести
        if (questContainer != null)
            questContainer.SetActive(false);
    }

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

        // При відкритті паузи за замовчуванням показуємо статистику, а квести ховаємо
        if (statisticsContainer != null) statisticsContainer.SetActive(true);
        if (questContainer != null) questContainer.SetActive(false);

        Time.timeScale = 0f;
        isPaused = true;

        // Оновлюємо статистику при відкритті паузи
        if (StatsUIManager.Instance != null)
        {
            StatsUIManager.Instance.UpdateStatsUI();
        }
    }

    // === МЕТОД ДЛЯ КНОПКИ "QUEST" ===
    public void OpenQuestClicked()
    {
        if (statisticsContainer != null) statisticsContainer.SetActive(false); // Ховаємо статистику
        if (questContainer != null) questContainer.SetActive(true);           // Вмикаємо квести

        // === ОНОВЛЕНО: Просто даємо команду інтерфейсу малювати свій ручний список ===
        if (questListManager != null)
        {
            questListManager.RefreshList();
        }
    }

    // === МЕТОД ДЛЯ КНОПКИ ЗАКРИТТЯ КВЕСТІВ ===
    public void CloseQuestClicked()
    {
        if (statisticsContainer != null) statisticsContainer.SetActive(true);  // Повертаємо статистику на екран
        if (questContainer != null) questContainer.SetActive(false);           // Ховаємо панель квестів
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        fader.FadeTo(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        fader.FadeTo("MainMenu");
    }
}