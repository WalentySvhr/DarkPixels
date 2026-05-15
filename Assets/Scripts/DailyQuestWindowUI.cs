using UnityEngine;
using System.Collections.Generic;

public class DailyQuestWindowUI : MonoBehaviour
{
    [Header("References")]
    public GameObject windowPanel; // Сама панель, яку будемо вмикати/вимикати
    public Transform questListContainer; // Де будуть спавнитись слоти
    public GameObject questSlotPrefab; // Префаб DailyQuestSlot

    // Список створених слотів, щоб не створювати їх безкінечно, а просто оновлювати
    private List<DailyQuestSlotUI> spawnedSlots = new List<DailyQuestSlotUI>();

    private void Start()
    {
        // Гарантуємо, що при старті вікно закрите
        windowPanel.SetActive(false);
    }

    // Цей метод ми повісимо на кнопку-календар
    public void ToggleWindow()
    {
        bool isActive = windowPanel.activeSelf;
        windowPanel.SetActive(!isActive);

        if (!isActive)
        {
            // Якщо ми тільки що ВІДКРИЛИ вікно, оновлюємо інфу
            RefreshUI();
        }
    }

    // Цей метод повісимо на кнопку "Х" (закрити)
    public void CloseWindow()
    {
        windowPanel.SetActive(false);
    }

    public void RefreshUI()
    {
        if (DailyQuestManager.Instance == null) return;

        var activeQuests = DailyQuestManager.Instance.activeDailies;

        // Якщо слотів ще не вистачає (перший раз відкрили), створюємо їх
        while (spawnedSlots.Count < activeQuests.Count)
        {
            GameObject newSlotObj = Instantiate(questSlotPrefab, questListContainer);
            DailyQuestSlotUI slotUI = newSlotObj.GetComponent<DailyQuestSlotUI>();
            spawnedSlots.Add(slotUI);
        }

        // Оновлюємо інформацію в слотах
        for (int i = 0; i < activeQuests.Count; i++)
        {
            spawnedSlots[i].gameObject.SetActive(true);
            spawnedSlots[i].Setup(activeQuests[i], i);
        }

        // Ховаємо зайві слоти, якщо раптом їх стало менше (хоча їх завжди 3)
        for (int i = activeQuests.Count; i < spawnedSlots.Count; i++)
        {
            spawnedSlots[i].gameObject.SetActive(false);
        }
    }
}