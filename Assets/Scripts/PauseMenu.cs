using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

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
        if (pauseMenuUI == null) return;

        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        // === ГЛОБАЛЬНИЙ ЗАПОБІЖНИК ===
        // Меню паузи закрилося — зменшуємо лічильник
        UIManager.RegisterWindowClose();
    }

    public void Pause()
    {
        if (pauseMenuUI == null) return;

        pauseMenuUI.SetActive(true);

        // При відкритті паузи за замовчуванням показуємо статистику, а квести ховаємо
        if (statisticsContainer != null) statisticsContainer.SetActive(true);
        if (questContainer != null) questContainer.SetActive(false);

        Time.timeScale = 0f;
        isPaused = true;

        // === ГЛОБАЛЬНИЙ ЗАПОБІЖНИК ===
        // Меню паузи відкрилося — збільшуємо лічильник
        UIManager.RegisterWindowOpen();

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

        // Просто даємо команду інтерфейсу малювати свій ручний список
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

        // ЗНАХОДИМО UIManager НА СЦЕНІ ТА ВИКЛИКАЄМО ЙОГО МЕТОД ЗАХИСТУ
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.ForceResetCounter();
        }

        fader.FadeTo(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;

        // ЗНАХОДИМО UIManager НА СЦЕНІ ТА ВИКЛИКАЄМО ЙОГО МЕТОД ЗАХИСТУ
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.ForceResetCounter();
        }

        Screen.orientation = ScreenOrientation.LandscapeLeft;
        fader.FadeTo("MainMenu");
    }

    void Update()
    {
        // Кнопка Escape (або Назад на Android) тепер керує паузою
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Якщо квести всередині паузи відкриті — кнопка Escape повертає нас до статистики
            if (isPaused && questContainer != null && questContainer.activeSelf)
            {
                CloseQuestClicked();
            }
            // Якщо відкрита просто чиста пауза (або гра йде) — працює як Toggle
            else
            {
                ToggleMenu();
            }
        }
    }
}