using UnityEngine;
using TMPro;

public class StatsUIManager : MonoBehaviour
{
    public static StatsUIManager Instance;

    [Header("Текстові поля статистики")]
    public TextMeshProUGUI maxFloorText;

    [Header("Формати тексту (Можна міняти в Інспекторі)")]
    [Tooltip("Використовуй {0} там, де має з'явитися цифра рекорду")]
    public string recordFoundFormat = "Рекорд Башні: <color=yellow>{0} поверх</color>";

    [Tooltip("Текст, який показується, якщо забігів ще не було")]
    public string noRecordFormat = "Рекорд Башні: <color=gray>0</color>";

    // --- ДЛЯ МАЙБУТНЬОГО ---
    // public TextMeshProUGUI totalCoinsText;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Цей метод просто оновлює текст. 
    // Ми будемо викликати його в момент натискання на паузу.
    public void UpdateStatsUI()
    {
        if (TowerManager.Instance != null && maxFloorText != null)
        {
            int record = TowerManager.Instance.maxFloorRecord;

            if (record > 0)
            {
                // Підставляємо значення record замість {0} у текст з Інспектора
                maxFloorText.text = string.Format(recordFoundFormat, record);
            }
            else
            {
                maxFloorText.text = noRecordFormat;
            }
        }
    }
}