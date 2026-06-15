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
    [Tooltip("Перетягни сюди SkillsPanel (твою нову панель вмінь)")]
    public GameObject skillsContainer;      // НОВЕ: Контейнер для скілів

    [Header("Менеджери Списків")]
    [Tooltip("Перетягни сюди сам QuestContainer, на якому висить скрипт QuestListUI")]
    public QuestListUI questListManager;
    [Tooltip("Перетягни сюди об'єкт SkillsPanel, на якому висить скрипт SkillsPanelWindow")]
    public SkillsPanelWindow skillsListManager; // НОВЕ: Менеджер списку скілів

    [Header("Посилання")]
    public SceneFader fader;                // Об'єкт FadePanel зі скриптом SceneFader
    private bool isPaused = false;

    void Start()
    {
        // При старті гри про всяк випадок ховаємо додаткові вкладки
        if (questContainer != null) questContainer.SetActive(false);
        if (skillsContainer != null) skillsContainer.SetActive(false); // НОВЕ
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
        UIManager.RegisterWindowClose();
    }

    public void Pause()
    {
        if (pauseMenuUI == null) return;

        pauseMenuUI.SetActive(true);

        // При відкритті паузи за замовчуванням показуємо статистику, а все інше ховаємо
        if (statisticsContainer != null) statisticsContainer.SetActive(true);
        if (questContainer != null) questContainer.SetActive(false);
        if (skillsContainer != null) skillsContainer.SetActive(false); // НОВЕ

        Time.timeScale = 0f;
        isPaused = true;

        // === ГЛОБАЛЬНИЙ ЗАПОБІЖНИК ===
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
        if (skillsContainer != null) skillsContainer.SetActive(false);         // Ховаємо скіли (НОВЕ)
        if (questContainer != null) questContainer.SetActive(true);             // Вмикаємо квести

        if (questListManager != null)
        {
            questListManager.RefreshList();
        }
    }

    // === МЕТОД ДЛЯ КНОПКИ ЗАКРИТТЯ КВЕСТІВ ===
    public void CloseQuestClicked()
    {
        if (statisticsContainer != null) statisticsContainer.SetActive(true);  // Повертаємо статистику
        if (questContainer != null) questContainer.SetActive(false);           // Ховаємо квести
    }

    // === НОВЕ: МЕТОД ДЛЯ КНОПКИ "SKILLS" ===
    public void OpenSkillsClicked()
    {
        if (statisticsContainer != null) statisticsContainer.SetActive(false); // Ховаємо статистику
        if (questContainer != null) questContainer.SetActive(false);           // Ховаємо квести
        if (skillsContainer != null) skillsContainer.SetActive(true);           // Вмикаємо скіли

        // Запускаємо генерацію префабів у вікні скілів
        if (skillsListManager != null)
        {
            skillsListManager.RefreshWindow();
        }
    }

    // === НОВЕ: МЕТОД ДЛЯ КНОПКИ ЗАКРИТТЯ СКІЛІВ ===
    public void CloseSkillsClicked()
    {
        if (statisticsContainer != null) statisticsContainer.SetActive(true);  // Повертаємо статистику
        if (skillsContainer != null) skillsContainer.SetActive(false);         // Ховаємо скіли
    }

    public void Restart()
    {
        Time.timeScale = 1f;

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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Якщо квести відкриті — Escape повертає до статистики
            if (isPaused && questContainer != null && questContainer.activeSelf)
            {
                CloseQuestClicked();
            }
            // НОВЕ: Якщо скіли відкриті — Escape також повертає до статистики
            else if (isPaused && skillsContainer != null && skillsContainer.activeSelf)
            {
                CloseSkillsClicked();
            }
            // Якщо відкрита чиста пауза — закриваємо меню/ставемо на паузу
            else
            {
                ToggleMenu();
            }
        }
    }
}