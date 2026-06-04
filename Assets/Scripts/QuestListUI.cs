using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class StoryQuestWrapper
{
    [Tooltip("Перетягни сюди файл квесту (звідси скрипт сам візьме назву квесту)")]
    public QuestData questData;

    [Tooltip("ID твого NPC на карті (наприклад: AlchemistNPC), до якого має вести стрілочка")]
    public string npcTargetID;

    [Tooltip("ОПЦІОНАЛЬНО: Квест, ПІСЛЯ виконання якого цей квест з'явиться у списку. Для самого першого квесту в ланцюжку залиш це поле ПУСТИМ (null).")]
    public QuestData prerequisiteQuest;
}

public class QuestListUI : MonoBehaviour
{
    [Header("Налаштування префабу")]
    public GameObject questSlotPrefab;
    public Transform contentParent;

    [Header("СПИСОК СЮЖЕТНИХ КВЕСТІВ")]
    [Tooltip("Натискай '+', додавай елементи і вибудовуй сюжетні ланцюжки!")]
    public List<StoryQuestWrapper> storyQuests = new List<StoryQuestWrapper>();

    void Awake()
    {
        if (contentParent == null) contentParent = transform;
    }

    public void RefreshList()
    {
        // Очищаємо старі кнопки
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // Створюємо нові кнопки з урахуванням ланцюжків
        foreach (StoryQuestWrapper item in storyQuests)
        {
            if (item.questData == null) continue;

            if (QuestManager.Instance != null)
            {
                // 1. ПЕРЕВІРКА: Якщо цей квест ВЖЕ ВИКОНАНИЙ — ховаємо його
                if (QuestManager.Instance.IsQuestCompleted(item.questData.name))
                {
                    continue;
                }

                // 2. ПЕРЕВІРКА ЛАНЦЮЖКА: Якщо вказано попередній квест, але він ЩЕ НЕ ВИКОНАНИЙ — ховаємо цей квест
                if (item.prerequisiteQuest != null && !QuestManager.Instance.IsQuestCompleted(item.prerequisiteQuest.name))
                {
                    continue; // Пропускаємо, бо час для цього квесту ще не настав
                }
            }

            // Якщо квест пройшов усі перевірки — створюємо для нього кнопку
            GameObject newSlot = Instantiate(questSlotPrefab, contentParent);
            QuestSlotUI slotScript = newSlot.GetComponent<QuestSlotUI>();

            if (slotScript != null)
            {
                slotScript.SetupQuestSlot(item.questData, item.npcTargetID);
            }
        }
    }
}