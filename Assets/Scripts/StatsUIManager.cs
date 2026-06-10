using UnityEngine;
using TMPro;

public class StatsUIManager : MonoBehaviour
{
    public static StatsUIManager Instance;

    [Header("Текстові поля статистики")]
    public TextMeshProUGUI maxFloorText;
    public TextMeshProUGUI maxKillsText; // Нове поле для вбитих монстрів

    [Header("Формати тексту Башні")]
    [Tooltip("Використовуй {0} там, де має з'явитися цифра рекорду поверху")]
    public string recordFoundFormat = "Рекорд Башні: <color=yellow>{0} поверх</color>";

    [Tooltip("Текст, який показується, якщо забігів ще не було")]
    public string noRecordFormat = "Рекорд Башні: <color=gray>0</color>";

    [Header("Формати тексту Вбивств")]
    [Tooltip("Використовуй {0} там, де має з'явитися цифра рекорду вбивств")]
    public string killsFoundFormat = "Макс. убито монстрів: <color=red>{0}</color>";

    [Tooltip("Текст, якщо ще нікого не вбили")]
    public string noKillsFormat = "Макс. убито монстрів: <color=gray>0</color>";

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Викликаємо цей метод при відкритті меню паузи / кінця гри
    public void UpdateStatsUI()
    {
        if (TowerManager.Instance == null) return;

        // 1. Оновлення рекорду поверху
        if (maxFloorText != null)
        {
            int floorRecord = TowerManager.Instance.maxFloorRecord;
            if (floorRecord > 0)
            {
                maxFloorText.text = string.Format(recordFoundFormat, floorRecord);
            }
            else
            {
                maxFloorText.text = noRecordFormat;
            }
        }

        // 2. Оновлення рекорду вбивств
        if (maxKillsText != null)
        {
            int killsRecord = TowerManager.Instance.maxKillsRecord; // Беремо дані з TowerManager
            if (killsRecord > 0)
            {
                maxKillsText.text = string.Format(killsFoundFormat, killsRecord);
            }
            else
            {
                maxKillsText.text = noKillsFormat;
            }
        }
    }
}